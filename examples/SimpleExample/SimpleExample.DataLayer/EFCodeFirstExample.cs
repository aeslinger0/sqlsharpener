using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace SimpleExample.EntityFrameworkCodeFirst
{
	public partial class TaskContext : DbContext
	{
		public TaskContext(): base()
		{
		}
		public DbSet<Tasks> Tasks { get; set; }
		public DbSet<TaskStatus> TaskStatus { get; set; }
	}


	[Table("Tasks")]
	public partial class Tasks
	{

		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Int32? Id { get; set; }

		[Required]
		[MaxLength(50)]
		public String Name { get; set; }

		[Required]
		[MaxLength(1000)]
		public String Description { get; set; }

		[Required]
		public Int32? TaskStatusId { get; set; }
		[ForeignKey("Id")]
		public virtual TaskStatus TaskStatus { get; set; }

		[Required]
		public DateTime? Created { get; set; }

		[Required]
		[MaxLength(50)]
		public String CreatedBy { get; set; }

		[Required]
		public DateTime? Updated { get; set; }

		[Required]
		[MaxLength(50)]
		public String UpdatedBy { get; set; }
	}

	[Table("TaskStatus")]
	public partial class TaskStatus
	{

		[Key]
		[Required]
		public Int32? Id { get; set; }
		public virtual ICollection<Tasks> Tasks { get; set; }

		[Required]
		[MaxLength(50)]
		public String Name { get; set; }
	}
}