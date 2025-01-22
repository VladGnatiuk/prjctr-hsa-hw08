# HW 08. InnoDB Indexes
## Initial setup
- To prepare 40m dataset run `GenerateData.exe`
- Copy .env.example to .env and put meaningful values
- Bring up containers `docker-compose up --build`
    - Turn down with data being persisted `docker-compose down`
    - Turn down with data being destroyed `docker-compose down -v`
- innodb_flush_log_at_trx_commit is defined in `docker-compose.yml`
- Query testing is performed from DB IDE
- Write testing is performed from command line as `docker-compose exec load-test bash /tests/load_test.sh`

## Compare performance of selection queries

### Without index
```SQL
SELECT
    user_id,
    name,
    date_of_birth,
    biography
FROM
    app_users
WHERE
    date_of_birth BETWEEN '1950-01-01' AND '1959-12-31'
ORDER BY
    date_of_birth ASC
LIMIT 10
[2025-01-22 12:22:20] 10 rows retrieved starting from 1 in 52 s 865 ms (execution: 52 s 628 ms, fetching: 237 ms)
```


### With BTREE index
```SQL
CREATE INDEX idx_date_of_birth USING BTREE ON app_users(date_of_birth)
[2025-01-22 12:26:16] completed in 3 m 21 s 991 ms


SELECT
    user_id,
    name,
    date_of_birth,
    biography
FROM
    app_users
WHERE
    date_of_birth BETWEEN '1950-01-01' AND '1959-12-31'
ORDER BY
    date_of_birth ASC
LIMIT 10
[2025-01-22 12:43:45] 10 rows retrieved starting from 1 in 439 ms (execution: 23 ms, fetching: 416 ms)
```


### With HASH  index
```SQL
DROP INDEX idx_date_of_birth ON app_users
[2025-01-22 12:44:33] completed in 37 ms

CREATE INDEX idx_date_of_birth USING HASH ON app_users(date_of_birth)
[2025-01-22 12:48:20] [HY000][3502] This storage engine does not support the HASH index algorithm, storage engine default was used instead.
[2025-01-22 12:48:20] completed in 3 m 34 s 381 ms

SELECT
    user_id,
    name,
    date_of_birth,
    biography
FROM
    app_users
WHERE
    date_of_birth BETWEEN '1950-01-01' AND '1959-12-31'
ORDER BY
    date_of_birth ASC
LIMIT 10

[2025-01-22 12:57:39] 10 rows retrieved starting from 1 in 268 ms (execution: 16 ms, fetching: 252 ms)
```

## Compare performance of write operations

```bash
siege -c 50 -r 5 -H "Content-Type: application/json" -f ./urls.txt -b
```

### innodb-flush-log-at-trx-commit=1

```json
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        23.92,
        "data_transferred":                     0.23,
        "response_time":                        4.38,
        "transaction_rate":                    10.45,
        "throughput":                           0.01,
        "concurrency":                         45.80,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  5.03,
        "shortest_transaction":                 0.94
},
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        23.86,
        "data_transferred":                     0.23,
        "response_time":                        4.35,
        "transaction_rate":                    10.48,
        "throughput":                           0.01,
        "concurrency":                         45.60,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  8.58,
        "shortest_transaction":                 0.94
}
```

### innodb-flush-log-at-trx-commit=2

```json
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        12.53,
        "data_transferred":                     0.23,
        "response_time":                        2.26,
        "transaction_rate":                    19.95,
        "throughput":                           0.02,
        "concurrency":                         45.07,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  4.61,
        "shortest_transaction":                 0.46
},
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        12.28,
        "data_transferred":                     0.23,
        "response_time":                        2.22,
        "transaction_rate":                    20.36,
        "throughput":                           0.02,
        "concurrency":                         45.16,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  5.00,
        "shortest_transaction":                 0.44
}
```

### innodb-flush-log-at-trx-commit=0
```json
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        12.41,
        "data_transferred":                     0.23,
        "response_time":                        2.21,
        "transaction_rate":                    20.15,
        "throughput":                           0.02,
        "concurrency":                         44.61,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  4.60,
        "shortest_transaction":                 0.45
},
{       "transactions":                          250,
        "availability":                       100.00,
        "elapsed_time":                        12.02,
        "data_transferred":                     0.23,
        "response_time":                        2.14,
        "transaction_rate":                    20.80,
        "throughput":                           0.02,
        "concurrency":                         44.45,
        "successful_transactions":               250,
        "failed_transactions":                     0,
        "longest_transaction":                  4.88,
        "shortest_transaction":                 0.46
}
```
