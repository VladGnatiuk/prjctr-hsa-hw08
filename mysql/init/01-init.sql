-- Create a test table
CREATE TABLE IF NOT EXISTS app_users (
    user_id BIGINT PRIMARY KEY AUTO_INCREMENT,
    date_of_birth DATE NOT NULL,
    name VARCHAR(255) NOT NULL,
    biography TEXT
); 