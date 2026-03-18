#!/usr/bin/env python3
"""
seed_from_csv.py – Leest IKEA_SA_Furniture_Web_Scrapings_sss.csv en vult
de dt_website database met categories, subcategories en products.

Vereisten:
    pip install psycopg2-binary

Gebruik:
    # Standaard (localhost:5432, user=postgres, db=dt_website)
    python database/seed_from_csv.py

    # Met wachtwoord
    python database/seed_from_csv.py --password JOUW_WACHTWOORD

    # Alle opties
    python database/seed_from_csv.py --host localhost --port 5432 --dbname dt_website --user postgres --password SECRET

    # Bestaande producten/categorieën eerst verwijderen voor een schone herstart
    python database/seed_from_csv.py --truncate
"""

import argparse
import csv
import os
import sys
from pathlib import Path

try:
    import psycopg2
    from psycopg2.extras import execute_batch
except ImportError:
    sys.exit(
        "psycopg2 is niet geïnstalleerd.\n"
        "Installeer het via:  pip install psycopg2-binary"
    )

SCRIPT_DIR = Path(__file__).parent
CSV_FILE = SCRIPT_DIR / "IKEA_SA_Furniture_Web_Scrapings_sss.csv"

# Maximale lengtes passend bij de VARCHAR-kolommen in het schema
MAX_CATEGORY_NAME = 50
MAX_SUBCATEGORY_NAME = 50
MAX_PRODUCT_NAME = 50


def parse_args():
    # Hier definiëren we alle command line opties die je kunt meegeven
    # als je dit script start in de terminal.
    p = argparse.ArgumentParser(
        description="Vul dt_website database met IKEA-productdata uit CSV"
    )
    p.add_argument("--host", default="localhost", help="PostgreSQL host (default: localhost)")
    p.add_argument("--port", type=int, default=5432, help="PostgreSQL poort (default: 5432)")
    p.add_argument("--dbname", default="dt_website", help="Databasenaam (default: dt_website)")
    p.add_argument("--user", default="postgres", help="PostgreSQL gebruiker (default: postgres)")
    p.add_argument(
        "--password",
        default=os.environ.get("PGPASSWORD", ""),
        help="Wachtwoord (default: PGPASSWORD omgevingsvariabele)",
    )
    p.add_argument(
        "--truncate",
        action="store_true",
        help="Verwijder bestaande rijen in products, subcategories en categories voor het invoegen",
    )
    return p.parse_args()


def extract_subcategory(short_description: str):
    """
    Extraheer het producttype uit short_description.
    De beschrijving heeft het formaat:  "  Bar stool with backrest,  74 cm"
    We nemen het deel vóór de eerste komma als subcategorie, bijv. "Bar stool with backrest".
    """
    if not short_description:
        return None
    first_part = short_description.strip().split(",")[0].strip()
    return first_part[:MAX_SUBCATEGORY_NAME] if first_part else None


def get_or_insert_category(cur, name, cache):
    """Geef category-id terug; voeg in als nog niet aanwezig."""
    # 1) Eerst proberen uit lokale cache (sneller, minder database-calls).
    if name in cache:
        return cache[name]
    # 2) Niet in cache? Dan in database zoeken.
    cur.execute('SELECT id FROM categories WHERE name = %s', (name,))
    row = cur.fetchone()
    if row:
        # Gevonden: ook in cache zetten voor volgende producten.
        cache[name] = row[0]
        return row[0]
    # 3) Nog steeds niet gevonden: nieuwe categorie aanmaken.
    cur.execute(
        'INSERT INTO categories (name, description) VALUES (%s, NULL) RETURNING id',
        (name,),
    )
    cat_id = cur.fetchone()[0]
    cache[name] = cat_id
    return cat_id


def get_or_insert_subcategory(cur, category_id, name, cache):
    """Geef subcategory-id terug; voeg in als nog niet aanwezig."""
    # Subcategorie is afhankelijk van categorie, daarom gebruiken we
    # (category_id, name) als unieke sleutel in de cache.
    key = (category_id, name)
    if key in cache:
        return cache[key]
    cur.execute(
        'SELECT id FROM subcategories WHERE category_id = %s AND name = %s',
        (category_id, name),
    )
    row = cur.fetchone()
    if row:
        cache[key] = row[0]
        return row[0]
    cur.execute(
        'INSERT INTO subcategories (category_id, name, description) VALUES (%s, %s, NULL) RETURNING id',
        (category_id, name),
    )
    sub_id = cur.fetchone()[0]
    cache[key] = sub_id
    return sub_id


def make_product_key(name, description, price, category_id, subcategory_id):
    """Maak een vergelijkbare sleutel om dubbele producten te herkennen."""
    # Prijs afronden op 2 decimalen zodat vergelijking past bij DECIMAL(10,2).
    normalized_price = round(float(price), 2)
    return (name, description, normalized_price, category_id, subcategory_id)


def main():
    # Lees alle argumenten die de gebruiker meegaf bij het starten.
    args = parse_args()

    if not CSV_FILE.exists():
        sys.exit(f"CSV-bestand niet gevonden: {CSV_FILE}")

    # --- CSV lezen ---
    # We lezen eerst alle rijen in memory, zodat we daarna in één flow
    # de database kunnen vullen.
    print(f"CSV lezen: {CSV_FILE.name} ...")
    rows = []
    with CSV_FILE.open(newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            rows.append(row)
    print(f"  {len(rows)} rijen geladen.")

    # --- Verbinding maken ---
    # Open een verbinding met PostgreSQL op basis van de CLI-opties.
    print(f"Verbinding maken met {args.user}@{args.host}:{args.port}/{args.dbname} ...")
    try:
        conn = psycopg2.connect(
            host=args.host,
            port=args.port,
            dbname=args.dbname,
            user=args.user,
            password=args.password,
        )
    except psycopg2.OperationalError as e:
        sys.exit(f"Verbinding mislukt: {e}")

    # We werken met een expliciete transactie.
    # Voordeel: bij fout kunnen we alles terugdraaien via rollback.
    conn.autocommit = False
    cur = conn.cursor()

    try:
        # --- Optioneel: tabellen leegmaken ---
        if args.truncate:
            print("Bestaande data verwijderen (--truncate) ...")
            # Verwijder in de juiste volgorde vanwege foreign keys
            cur.execute("DELETE FROM order_items")
            cur.execute("DELETE FROM cart_items")
            cur.execute("DELETE FROM reviews")
            cur.execute("DELETE FROM products")
            cur.execute("DELETE FROM subcategories")
            cur.execute("DELETE FROM categories")
            print("  Tabellen leeggemaakt.")

        # --- Caches voor deduplicatie ---
        # Deze dictionaries voorkomen dat we dezelfde categorie/subcategorie
        # meerdere keren moeten opzoeken of invoegen.
        category_cache = {}     # category_name -> id
        subcategory_cache = {}  # (category_id, sub_name) -> id

        # --- Producten verwerken ---
        # product_rows bewaren we tijdelijk en schrijven we straks in batch weg.
        product_rows = []
        skipped = 0
        duplicates_skipped = 0

        # Bestaande producten ophalen zodat opnieuw importeren geen duplicaten maakt.
        cur.execute(
            """
            SELECT name, description, price, category_id, subcategory_id
            FROM products
            """
        )
        existing_product_keys = {
            make_product_key(name, description, price, category_id, subcategory_id)
            for name, description, price, category_id, subcategory_id in cur.fetchall()
        }

        print("Categorieën, subcategorieën en producten verwerken ...")
        for row in rows:
            # Prijs is verplicht (NOT NULL in schema); sla rij over als ontbrekend
            price_raw = row.get("price", "").strip()
            try:
                price = float(price_raw)
            except ValueError:
                # Onbruikbare prijs? Deze rij telt als overgeslagen,
                # maar de rest gaat gewoon door.
                skipped += 1
                continue

            # --- Category ---
            cat_raw = row.get("category", "").strip()
            cat_name = cat_raw[:MAX_CATEGORY_NAME] if cat_raw else None
            cat_id = get_or_insert_category(cur, cat_name, category_cache) if cat_name else None

            # --- Subcategory (producttype uit short_description) ---
            sub_name = extract_subcategory(row.get("short_description", ""))
            sub_id = None
            if sub_name and cat_id is not None:
                sub_id = get_or_insert_subcategory(cur, cat_id, sub_name, subcategory_cache)

            # --- Product ---
            # Name en description normaliseren voordat ze de database in gaan.
            product_name = row.get("name", "").strip()[:MAX_PRODUCT_NAME]
            description = row.get("short_description", "").strip() or None

            product_key = make_product_key(product_name, description, price, cat_id, sub_id)
            if product_key in existing_product_keys:
                # Al aanwezig in database of eerder in deze run toegevoegd.
                duplicates_skipped += 1
                continue

            existing_product_keys.add(product_key)

            product_rows.append((product_name, description, price, cat_id, sub_id))

        # --- Batch-insert producten ---
        # execute_batch is sneller dan per product een losse INSERT.
        execute_batch(
            cur,
            """
            INSERT INTO products (name, description, price, category_id, subcategory_id)
            VALUES (%s, %s, %s, %s, %s)
            """,
            product_rows,
            page_size=500,
        )

        conn.commit()

        # --- Samenvatting ---
        cur.execute("SELECT COUNT(*) FROM categories")
        n_cats = cur.fetchone()[0]
        cur.execute("SELECT COUNT(*) FROM subcategories")
        n_subs = cur.fetchone()[0]
        cur.execute("SELECT COUNT(*) FROM products")
        n_prods = cur.fetchone()[0]

        print()
        print("=== Klaar! ===")
        print(f"  Categorieën:     {n_cats}")
        print(f"  Subcategorieën:  {n_subs}")
        print(f"  Producten:       {n_prods}")
        if skipped:
            print(f"  Overgeslagen:    {skipped} (geen geldige prijs)")
        if duplicates_skipped:
            print(f"  Dubbelen:        {duplicates_skipped} (al aanwezig)")

    except Exception as e:
        # Bij een onverwachte fout draaien we alle wijzigingen terug.
        conn.rollback()
        cur.close()
        conn.close()
        sys.exit(f"Fout tijdens invoegen: {e}")

    # Resources altijd sluiten als alles goed ging.
    cur.close()
    conn.close()


if __name__ == "__main__":
    main()