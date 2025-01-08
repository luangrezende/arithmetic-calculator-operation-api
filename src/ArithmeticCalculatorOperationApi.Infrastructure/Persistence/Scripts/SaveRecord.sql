INSERT INTO operation_record (
    id, 
    user_id, 
    cost, 
    user_balance, 
    result, 
    expression, 
    created_at
) VALUES (
    @Id, 
    @UserId, 
    @Cost, 
    @UserBalance, 
    @Result, 
    @Expression, 
    @CreatedAt
);
