CREATE PROCEDURE [dbo].[usp_TaskCreateMultiple]
	@tasks Tasks READONLY
AS
	DECLARE @ids TABLE(id int)

	INSERT INTO Tasks
		(Name, [Description], TaskStatusId, Created, CreatedBy, Updated, UpdatedBy)
	OUTPUT inserted.Id into @ids
	SELECT
		Name, [Description], TaskStatusId, Created, CreatedBy, Updated, UpdatedBy
	FROM @tasks


	SELECT id from @ids