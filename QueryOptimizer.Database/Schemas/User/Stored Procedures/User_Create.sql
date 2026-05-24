CREATE PROCEDURE [User].[User_Create]
	@UserName NVARCHAR(255)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT NULL FROM [User].[User] us WITH(NOLOCK) WHERE us.UserName = @UserName)
	BEGIN
		RAISERROR('User with the same name already exists.', 16, 1);
		RETURN;
	END

	BEGIN TRANSACTION;

	INSERT INTO [User].[User]
	(UserName)
	VALUES
	(@UserName);

	COMMIT TRANSACTION;

	DECLARE @NewID INT;

	SELECT @NewID = ISNULL(us.Id, 0)
	FROM [User].[User] us WITH(NOLOCK)
	WHERE us.UserName = @UserName;
END
