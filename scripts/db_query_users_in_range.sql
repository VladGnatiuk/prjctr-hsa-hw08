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
LIMIT 10; 