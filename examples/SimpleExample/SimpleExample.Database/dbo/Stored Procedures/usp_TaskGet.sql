CREATE PROCEDURE [dbo].[usp_TaskGet]
	@TaskId int
AS

	-- Specifying "TOP 1" makes the generated return value a single instance instead of an array.
	SELECT TOP 1
		t.Name,
		t.[Description],
		ts.Name as [Status],
		t.Created,
		t.CreatedBy,
		t.Updated,
		t.UpdatedBy
	FROM Tasks t
	JOIN TaskStatus ts ON t.TaskStatusId = ts.Id
	WHERE t.Id = @TaskId