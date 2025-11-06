-- This script creates a 'users' table in PostgreSQL and populates it with sample data.

-- ---
-- Part 1: Create the Table Structure
-- ---

CREATE TABLE IF NOT EXISTS users (
    -- 'SERIAL' creates an auto-incrementing integer, which is ideal for a primary key.
    id SERIAL PRIMARY KEY,
    
    -- 'VARCHAR(255)' provides a reasonable limit for a full name.
    -- 'NOT NULL' ensures this field cannot be empty.
    fullName VARCHAR(255) NOT NULL,
    
    -- 'UNIQUE' ensures that no two users can have the same email address.
    email VARCHAR(255) UNIQUE NOT NULL,
    
    -- 'VARCHAR(50)' is used for the role.
    -- We set a 'DEFAULT' value of 'user' for new entries.
    -- NOTE: You might consider using an ENUM type for more strict role control, e.g.:
    -- CREATE TYPE user_role AS ENUM ('user', 'admin', 'guest');
    -- role user_role NOT NULL DEFAULT 'user',
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    
    -- This field is for storing a *hashed* password, not a plaintext one.
    -- A length of 255 is sufficient for most modern hashing algorithm outputs (e.g., bcrypt).
    password VARCHAR(255) NOT NULL,
    
    -- Optional: Add timestamps to track when records are created or updated.
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Optional: Create an index on the email column for faster lookups.
-- The UNIQUE constraint on 'email' often creates an index automatically,
-- but explicitly creating one is good practice for clarity.
CREATE INDEX IF NOT EXISTS idx_users_email ON users (email);

-- Optional: A trigger to automatically update the 'updated_at' timestamp
-- whenever a row is modified.

CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Drop the trigger if it already exists to avoid errors on re-run
DROP TRIGGER IF EXISTS set_timestamp ON users;

-- Create the trigger
CREATE TRIGGER set_timestamp
BEFORE UPDATE ON users
FOR EACH ROW
EXECUTE PROCEDURE trigger_set_timestamp();


-- ---
-- Part 2: Populate the Table
-- ---

-- WARNING:
-- This script is inserting PLAINTEXT passwords. This is a
-- SEVERE SECURITY RISK and should ONLY be done for temporary,
-- local testing where security is not a concern.
--
-- In a real application, you MUST hash passwords using a strong
-- algorithm like bcrypt before storing them.
-- The 'password' column should store a HASHED password, not a plaintext one.
-- These examples use simple strings as placeholders. In a real application,
-- you would use a library (e.g., bcrypt) to generate a secure hash
-- and store that hash here.

-- Insert multiple users at once
INSERT INTO users (fullName, email, role, password)
VALUES
    (
        'Alice Smith',
        'alice@example.com',
        'admin',
        'adminpass123' -- Simple plaintext password
    ),
    (
        'Bob Johnson',
        'bob@example.com',
        'user',
        'userpass456'  -- Simple plaintext password
    ),
    (
        'Charlie Brown',
        'charlie@example.com',
        DEFAULT, -- This will use the default value 'user'
        'charlie789' -- Simple plaintext password
    );

-- You can also insert users one by one
INSERT INTO users (fullName, email, password)
VALUES
    (
        'David Lee',
        'david@example.com',
        'davidpass' -- Simple plaintext password
    ),
    (
        'Emily White',
        'emily@example.com',
        'emilypass' -- Simple plaintext password
    ),
    (
        'Michael Chen',
        'michael@example.com',
        'michaelpass' -- Simple plaintext password
    ),
    (
        'Sarah Green',
        'sarah@example.com',
        'sarah-admin' -- Simple plaintext password
    );
-- Note: The 'role' for David Lee will automatically be set to 'user'
-- because of the DEFAULT constraint we defined in the CREATE TABLE script.


-- ---
-- Part 3: Verify the Data
-- ---

-- Verify the inserted data
SELECT id, fullName, email, role, created_at
FROM users;
