using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; } = false;
}
