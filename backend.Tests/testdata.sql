-- INSERT INTO customers (id, user_id, first_name)
-- VALUES (1, 1, 'John');

-- INSERT INTO orders (id, customer_id, status)
-- VALUES (1, 1, 'Pending');

-- INSERT INTO order_status_history (order_id, status)
-- VALUES (1, 'Pending');


-- Users
INSERT INTO "users"
("email", "password_hash", "role")
VALUES
('customer1@test.com', 'hashedpassword', 'Customer'),
('customer2@test.com', 'hashedpassword', 'Customer'),
('admin@test.com', 'hashedpassword', 'Admin');

-- Customers
INSERT INTO "customers"
("user_id", "first_name", "last_name", "phone", "address")
VALUES
(1, 'John', 'Doe', '0611111111', 'Amsterdam 1'),
(2, 'Jane', 'Smith', '0622222222', 'Amsterdam 2');

-- Employees

-- Shopping carts
INSERT INTO "shopping_carts"
("customer_id")
VALUES
(1),
(2);

-- Cart items
INSERT INTO "cart_items"
("cart_id", "product_id", "quantity")
VALUES
(1, 1, 2),
(1, 2, 1),
(2, 1, 3);

-- Orders
INSERT INTO "orders"
( "customer_id", "status")
VALUES
( 1, 'Pending'),
( 1, 'Processing'),
( 2, 'Completed');

-- Order items
INSERT INTO "order_items"
("order_id", "product_id", "quantity", "price")
VALUES
(1, 1, 2, 19.99),
(1, 2, 1, 49.99),
(2, 1, 3, 19.99),
(3, 2, 2, 49.99);

-- Order status history
INSERT INTO "order_status_history"
("order_id", "status")
VALUES
(1, 'Pending'),
(2, 'Pending'),
(2, 'Processing'),
(3, 'Pending'),
(3, 'Processing'),
(3, 'Completed');

-- Payments
-- INSERT INTO "payments"
-- ("order_id", "amount", "payment_method", "status")
-- VALUES
-- (1, 89.97, 'Credit Card', 'Pending'),
-- (2, 59.97, 'PayPal', 'Completed'),
-- (3, 99.98, 'iDEAL', 'Completed');