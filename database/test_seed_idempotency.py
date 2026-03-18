#!/usr/bin/env python3
"""
Test dat seed_from_csv.py idempotent is voor products.

Wat dit script doet:
1. Draait seed_from_csv.py met --truncate (schone start)
2. Leest product count
3. Draait seed_from_csv.py opnieuw zonder --truncate
4. Leest product count opnieuw
5. Controleert of product count gelijk is gebleven
6. Controleert of er geen dubbele product records bestaan

Exit code:
- 0: test geslaagd
- 1: test gefaald
"""

import argparse
import os
import subprocess
import sys
from pathlib import Path

import psycopg2


ROOT_DIR = Path(__file__).resolve().parents[1]
SEED_SCRIPT = ROOT_DIR / "database" / "seed_from_csv.py"


def parse_args():
    # Zelfde connectie-opties als het seed script, zodat de test flexibel blijft
    # voor lokale omgeving of een andere database host.
    parser = argparse.ArgumentParser(description="Test dubbele import van seed_from_csv.py")
    parser.add_argument("--host", default="localhost", help="PostgreSQL host")
    parser.add_argument("--port", type=int, default=5432, help="PostgreSQL poort")
    parser.add_argument("--dbname", default="dt_website", help="Databasenaam")
    parser.add_argument("--user", default="postgres", help="PostgreSQL gebruiker")
    parser.add_argument(
        "--password",
        default=os.environ.get("PGPASSWORD", ""),
        help="Wachtwoord (default: PGPASSWORD)",
    )
    return parser.parse_args()


def run_seed(args, truncate=False):
    # Bouw het commando op dat seed_from_csv.py start met dezelfde DB-parameters.
    cmd = [
        sys.executable,
        str(SEED_SCRIPT),
        "--host",
        args.host,
        "--port",
        str(args.port),
        "--dbname",
        args.dbname,
        "--user",
        args.user,
    ]

    if args.password:
        cmd.extend(["--password", args.password])

    if truncate:
        # --truncate zorgt voor een schone test-start.
        cmd.append("--truncate")

    print("\n[RUN]", " ".join(cmd))
    # check=False: we willen zelf de foutmelding en RuntimeError afhandelen.
    result = subprocess.run(cmd, cwd=str(ROOT_DIR), check=False)
    if result.returncode != 0:
        raise RuntimeError("seed_from_csv.py faalde")


def get_product_count(conn):
    # Totaal aantal producten in de tabel, gebruikt voor vergelijking run 1 vs run 2.
    with conn.cursor() as cur:
        cur.execute("SELECT COUNT(*) FROM products")
        return cur.fetchone()[0]


def get_duplicate_group_count(conn):
    # Zoek groepen records die exact dezelfde productvelden hebben.
    # Als dit 0 is, zijn er geen echte duplicaten in de products-tabel.
    query = """
    SELECT COUNT(*)
    FROM (
        SELECT name, description, price, category_id, subcategory_id
        FROM products
        GROUP BY name, description, price, category_id, subcategory_id
        HAVING COUNT(*) > 1
    ) dup
    """
    with conn.cursor() as cur:
        cur.execute(query)
        return cur.fetchone()[0]


def main():
    args = parse_args()

    if not SEED_SCRIPT.exists():
        print("[FAIL] seed script niet gevonden:", SEED_SCRIPT)
        return 1

    try:
        # 1) Schone start + eerste import (baseline)
        run_seed(args, truncate=True)

        conn = psycopg2.connect(
            host=args.host,
            port=args.port,
            dbname=args.dbname,
            user=args.user,
            password=args.password,
        )
        conn.autocommit = True

        # Meet baseline na de eerste import.
        count_after_first = get_product_count(conn)
        print(f"[INFO] Product count na eerste import: {count_after_first}")

        # 2) Tweede import zonder truncate (moet niets extra's toevoegen)
        run_seed(args, truncate=False)

        # Product count moet gelijk blijven als import idempotent is.
        count_after_second = get_product_count(conn)
        print(f"[INFO] Product count na tweede import: {count_after_second}")

        # Extra veiligheidscheck: zelfs bij gelijk count willen we geen dubbele groepen.
        duplicate_groups = get_duplicate_group_count(conn)
        print(f"[INFO] Aantal duplicate groepen in products: {duplicate_groups}")

        same_count = count_after_first == count_after_second
        no_duplicates = duplicate_groups == 0

        # Alleen PASS als beide voorwaarden waar zijn.
        if same_count and no_duplicates:
            print("[PASS] Geen dubbele producten na tweede import.")
            conn.close()
            return 0

        # Anders duidelijke FAIL met reden(en).
        print("[FAIL] Dubbel-import check is niet geslaagd.")
        if not same_count:
            print("[FAIL] Product count is veranderd tussen import 1 en 2.")
        if not no_duplicates:
            print("[FAIL] Er bestaan duplicate groepen in products.")

        conn.close()
        return 1

    except Exception as exc:
        # Vang onverwachte fouten af zodat CI/terminal een duidelijke fail ziet.
        print(f"[FAIL] Test crashte: {exc}")
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
