namespace eGrants.Models
{
    public class Person
    {
        public string PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiName { get; set; }

        public ICollection<PersonAddress> Addresses { get; set; }

        //public ICollection<person_involvements_mv> PersonInvolvements { get; set; }
    }
}
