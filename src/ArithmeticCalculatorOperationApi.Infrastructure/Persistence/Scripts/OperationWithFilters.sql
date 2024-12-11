SELECT 
        r.id, 
        r.user_id, 
        r.cost, 
        r.user_balance, 
        r.result, 
        r.expression, 
        r.created_at
    FROM operation_record r
    WHERE 
        r.deleted_at IS NULL AND
        r.user_id = @UserId AND (
            @Query = '' OR (
                LOWER(r.result) LIKE CONCAT('%', LOWER(@Query), '%') OR
                LOWER(r.expression) LIKE CONCAT('%', LOWER(@Query), '%') OR
                r.cost LIKE CONCAT('%', @Query, '%') OR
                (
                    REPLACE(DATE_FORMAT(r.created_at, '%Y/%m/%d'), '/', '') LIKE REPLACE(CONCAT('%', @Query, '%'), '/', '')
                    OR DATE_FORMAT(r.created_at, '%Y-%m-%d %H:%i:%s') LIKE CONCAT('%', @Query, '%')
                )
            )
        )