CREATE PROCEDURE [dbo].[usp_TaskUpdate]
	@TaskId int,
	@Name varchar(50),
	@Description varchar(1000),
	@TaskStatusId int,
	@Updated datetime,
	@UpdatedBy varchar(50)
AS

	UPDATE Tasks
	SET
		Name = @Name,
		[Description] = @Description,
		TaskStatusId = @TaskStatusId,
		Updated = @Updated,
		UpdatedBy = @UpdatedBy
	WHERE
		Id = @TaskId
