CREATE PROCEDURE [dbo].[usp_TaskCreate]
	@Name varchar(50),
	@Description varchar(1000),
	@TaskStatusId int,
	@Created datetime,
	@CreatedBy varchar(50),
	@Updated datetime,
	@UpdatedBy varchar(50),
	@TaskId int output
AS
	INSERT INTO Tasks
		(Name, [Description], TaskStatusId, Created, CreatedBy, Updated, UpdatedBy)
	VALUES
		(@Name, @Description, @TaskStatusId, @Created, @CreatedBy, @Updated, @UpdatedBy)

	-- Output parameters are generated as C# "out" parameters.
	SET @TaskId = SCOPE_IDENTITY()
