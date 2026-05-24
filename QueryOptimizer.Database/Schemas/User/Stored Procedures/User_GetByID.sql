CREATE PROCEDURE [User].[User_GetByID]
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		us.Id,
		us.UserName
	FROM [User].[User] us WITH(NOLOCK)
	WHERE us.Id = @Id;
END
