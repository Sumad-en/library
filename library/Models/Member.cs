namespace library.Models;

// One Book object holds one row of lending.book.
public class Members
{
    public long member_id { get; set; }          // member_id  BIGINT, the primary key
    public string full_name { get; set; } = "";   // full name     VARCHAR(150) NOT NULL
    public string? email { get; set; }    // email  VARCHAR(40), may be NULL
    public string? member_type { get; set; } = "";    // member type, NOT NULL

}