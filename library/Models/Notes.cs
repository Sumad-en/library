namespace library.Models
{
    public class Notes
    {
        public long note_id { get; set; }          // note_id   BIGINT, the primary key
        public string title { get; set; } = "";   // title     VARCHAR(150) NOT NULL
        public string? body { get; set; }     // category  VARCHAR(40), may be NULL
        public DateTime created_on { get; set; } = DateTime.UtcNow;       // Created_On NOT NULL

    }
}
