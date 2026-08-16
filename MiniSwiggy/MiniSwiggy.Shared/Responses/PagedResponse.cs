using System;
using System.Collections.Generic;
using System.Text;

namespace MiniSwiggy.Shared.Responses;

public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalRecords { get; init; }

    public int TotalPages { get; init; }

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse()
    {
    }

    public PagedResponse(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message = "Success")
        : base(true, 200, message, data)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}
