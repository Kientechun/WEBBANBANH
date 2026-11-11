-- Schema generated from the InitialCreate EF migration. Start the app after import to seed demo data.
CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `migration_id` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `product_version` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `pk___ef_migrations_history` PRIMARY KEY (`migration_id`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `categories` (
    `id` int NOT NULL AUTO_INCREMENT,
    `name` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
    `slug` varchar(140) CHARACTER SET utf8mb4 NOT NULL,
    `description` longtext CHARACTER SET utf8mb4 NULL,
    `image` longtext CHARACTER SET utf8mb4 NULL,
    `display_order` int NOT NULL,
    `status` tinyint(1) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_categories` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `contacts` (
    `id` int NOT NULL AUTO_INCREMENT,
    `name` longtext CHARACTER SET utf8mb4 NOT NULL,
    `email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `phone` longtext CHARACTER SET utf8mb4 NULL,
    `subject` longtext CHARACTER SET utf8mb4 NOT NULL,
    `message` longtext CHARACTER SET utf8mb4 NOT NULL,
    `status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `created_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_contacts` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `news` (
    `id` int NOT NULL AUTO_INCREMENT,
    `title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `slug` varchar(220) CHARACTER SET utf8mb4 NOT NULL,
    `summary` longtext CHARACTER SET utf8mb4 NOT NULL,
    `content` longtext CHARACTER SET utf8mb4 NOT NULL,
    `image` longtext CHARACTER SET utf8mb4 NULL,
    `author` longtext CHARACTER SET utf8mb4 NOT NULL,
    `views` int NOT NULL,
    `status` tinyint(1) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_news` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `settings` (
    `id` int NOT NULL AUTO_INCREMENT,
    `setting_key` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `setting_value` longtext CHARACTER SET utf8mb4 NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_settings` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `users` (
    `id` int NOT NULL AUTO_INCREMENT,
    `name` varchar(120) CHARACTER SET utf8mb4 NOT NULL,
    `email` varchar(180) CHARACTER SET utf8mb4 NOT NULL,
    `password` longtext CHARACTER SET utf8mb4 NOT NULL,
    `phone` longtext CHARACTER SET utf8mb4 NULL,
    `address` longtext CHARACTER SET utf8mb4 NULL,
    `avatar` longtext CHARACTER SET utf8mb4 NULL,
    `role` longtext CHARACTER SET utf8mb4 NOT NULL,
    `status` tinyint(1) NOT NULL,
    `email_verified` tinyint(1) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_users` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `products` (
    `id` int NOT NULL AUTO_INCREMENT,
    `category_id` int NOT NULL,
    `name` varchar(180) CHARACTER SET utf8mb4 NOT NULL,
    `slug` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `description` longtext CHARACTER SET utf8mb4 NOT NULL,
    `ingredients` longtext CHARACTER SET utf8mb4 NOT NULL,
    `price` decimal(10,2) NOT NULL,
    `sale_price` decimal(10,2) NULL,
    `image` longtext CHARACTER SET utf8mb4 NULL,
    `gallery` json NOT NULL,
    `stock` int NOT NULL,
    `is_featured` tinyint(1) NOT NULL,
    `is_new` tinyint(1) NOT NULL,
    `is_bestseller` tinyint(1) NOT NULL,
    `views` int NOT NULL,
    `status` tinyint(1) NOT NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_products` PRIMARY KEY (`id`),
    CONSTRAINT `fk_products_categories_category_id` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `orders` (
    `id` int NOT NULL AUTO_INCREMENT,
    `user_id` int NULL,
    `order_number` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `customer_name` longtext CHARACTER SET utf8mb4 NOT NULL,
    `customer_email` longtext CHARACTER SET utf8mb4 NOT NULL,
    `customer_phone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `customer_address` longtext CHARACTER SET utf8mb4 NOT NULL,
    `total_amount` decimal(10,2) NOT NULL,
    `payment_method` longtext CHARACTER SET utf8mb4 NOT NULL,
    `payment_status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `transaction_id` longtext CHARACTER SET utf8mb4 NULL,
    `paid_at` datetime(6) NULL,
    `status` longtext CHARACTER SET utf8mb4 NOT NULL,
    `notes` longtext CHARACTER SET utf8mb4 NULL,
    `admin_notes` longtext CHARACTER SET utf8mb4 NULL,
    `created_at` datetime(6) NOT NULL,
    `updated_at` datetime(6) NOT NULL,
    CONSTRAINT `pk_orders` PRIMARY KEY (`id`),
    CONSTRAINT `fk_orders_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `cart` (
    `id` int NOT NULL AUTO_INCREMENT,
    `user_id` int NOT NULL,
    `product_id` int NOT NULL,
    `quantity` int NOT NULL,
    CONSTRAINT `pk_cart` PRIMARY KEY (`id`),
    CONSTRAINT `fk_cart_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_cart_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `order_items` (
    `id` int NOT NULL AUTO_INCREMENT,
    `order_id` int NOT NULL,
    `product_id` int NULL,
    `product_name` longtext CHARACTER SET utf8mb4 NOT NULL,
    `product_image` longtext CHARACTER SET utf8mb4 NULL,
    `price` decimal(10,2) NOT NULL,
    `quantity` int NOT NULL,
    `subtotal` decimal(10,2) NOT NULL,
    CONSTRAINT `pk_order_items` PRIMARY KEY (`id`),
    CONSTRAINT `fk_order_items_orders_order_id` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE,
    CONSTRAINT `fk_order_items_products_product_id` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE INDEX `ix_cart_product_id` ON `cart` (`product_id`);

CREATE UNIQUE INDEX `ix_cart_user_id_product_id` ON `cart` (`user_id`, `product_id`);

CREATE UNIQUE INDEX `ix_categories_slug` ON `categories` (`slug`);

CREATE UNIQUE INDEX `ix_news_slug` ON `news` (`slug`);

CREATE INDEX `ix_order_items_order_id` ON `order_items` (`order_id`);

CREATE INDEX `ix_order_items_product_id` ON `order_items` (`product_id`);

CREATE UNIQUE INDEX `ix_orders_order_number` ON `orders` (`order_number`);

CREATE INDEX `ix_orders_user_id` ON `orders` (`user_id`);

CREATE INDEX `ix_products_category_id_status` ON `products` (`category_id`, `status`);

CREATE INDEX `ix_products_is_featured` ON `products` (`is_featured`);

CREATE INDEX `ix_products_is_new` ON `products` (`is_new`);

CREATE UNIQUE INDEX `ix_products_slug` ON `products` (`slug`);

CREATE UNIQUE INDEX `ix_settings_setting_key` ON `settings` (`setting_key`);

CREATE UNIQUE INDEX `ix_users_email` ON `users` (`email`);

INSERT INTO `__EFMigrationsHistory` (`migration_id`, `product_version`)
VALUES ('20261110125225_InitialCreate', '8.0.11');

COMMIT;
