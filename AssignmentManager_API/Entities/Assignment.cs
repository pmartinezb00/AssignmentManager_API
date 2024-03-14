namespace TaskManagerApi.Entities
{
	public class Assignment
	{
        public int assignment_id_i { get; set; }
        public string assignment_title_nv { get; set; } = null!;
        public string assignment_description_nv { get; set; } = null!;
		public int? assignment_order_i { get; set; }
		public bool assignment_done_b { get; set; }
	}
}
