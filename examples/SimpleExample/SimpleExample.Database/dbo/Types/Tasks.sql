CREATE TYPE Tasks as TABLE(
	Name varchar(50),
	[Description] varchar(1000),
	TaskStatusId int,
	Created datetime,
	CreatedBy varchar(50),
	Updated datetime,
	UpdatedBy varchar(50)
)