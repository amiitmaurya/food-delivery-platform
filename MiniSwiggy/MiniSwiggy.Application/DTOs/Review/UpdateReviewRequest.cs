using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Application.DTOs.Review;

public class UpdateReviewRequest
{
    public int Rating { get; set; }

    public string? Comment { get; set; }
}
