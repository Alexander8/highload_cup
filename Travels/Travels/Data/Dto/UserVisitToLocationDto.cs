namespace Travels.Data.Dto
{
    public struct UserVisitToLocationDto
    {
        public int mark;
        public long visited_at;
        public string place;

        public UserVisitToLocationDto(int mark, long visited_at, string place)
        {
            this.mark = mark;
            this.visited_at = visited_at;
            this.place = place;
        }
    }
}
