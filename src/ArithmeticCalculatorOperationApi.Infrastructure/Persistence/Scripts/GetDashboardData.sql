 SELECT 
    -- Total number of operations for the user
    (SELECT COUNT(*) 
        FROM operation_record 
        WHERE user_id = @UserId) AS TotalOperations,
 
    -- Total number of operations in the current month
    (SELECT COUNT(*) 
        FROM operation_record 
        WHERE user_id = @UserId AND deleted_at IS NULL
        AND MONTH(created_at) = MONTH(CURRENT_DATE)
        AND YEAR(created_at) = YEAR(CURRENT_DATE)) AS TotalMonthlyOperations,

    -- Total credit added
    (SELECT COALESCE(SUM(amount), 0) 
        FROM balance_record br
        JOIN bank_account ba ON br.account_id = ba.id
        WHERE br.type = 'credit' AND ba.user_id = @UserId) AS TotalCredit,

    -- Total amount of money added in the current year
    (SELECT COALESCE(SUM(CAST(amount AS DECIMAL(15, 2))), 0) 
        FROM balance_record br
        JOIN bank_account ba ON br.account_id = ba.id
        WHERE ba.user_id = @UserId 
        AND br.type = 'credit'
        AND YEAR(br.created_at) = YEAR(CURRENT_DATE)) AS TotalAnnualCashAdded,

    -- Total number of operations on the platform
    (SELECT COUNT(*) 
        FROM operation_record) AS TotalPlatformOperations,

    -- Total amount of money spent on the platform
    (SELECT COALESCE(SUM(cost), 0) 
        FROM operation_record) AS TotalPlatformCashSpent,

    -- Total amount of money added to the platform
    (SELECT COALESCE(SUM(amount), 0) 
        FROM balance_record br
        JOIN bank_account ba ON br.account_id = ba.id
        WHERE br.type = 'credit') AS TotalPlatformCashAdded