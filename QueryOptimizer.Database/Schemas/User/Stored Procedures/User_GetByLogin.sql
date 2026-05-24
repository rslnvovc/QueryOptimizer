CREATE PROCEDURE [User].[User_GetByLogin]
	@UserName NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		us.Id,
		us.UserName
	FROM [User].[User] us WITH(NOLOCK)
	WHERE us.UserName = @UserName
END
