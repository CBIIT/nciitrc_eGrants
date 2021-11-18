
namespace egrants_new.Integration.EmailRulesEngine.Models
{
   public  interface IEmailAction
    {
        string ActionData { get; set; }
        EmailRule EmailRule { get; set; }
        EmailRuleActionResult DoAction(EmailMsg msg = null);

    }
}
