-- создание таблицы
CREATE TABLE IF NOT EXISTS Invoices (
    Id SERIAL PRIMARY KEY,
    BankName VARCHAR(100) NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Status VARCHAR(20) NOT NULL DEFAULT 'pending',
    UpdatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    RetryCount INT NOT NULL DEFAULT 0,
    LastAttemptAt TIMESTAMP NULL
);

-- генерация 10 тестовых записей
INSERT INTO Invoices (BankName, Amount, Status, UpdatedAt)
SELECT
    'Bank_' || i,
    ROUND((RANDOM() * 1000 + 10)::NUMERIC, 2),
    'pending',
    NOW() - INTERVAL '1 hour' * i
FROM generate_series(1, 10) AS s(i);