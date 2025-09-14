namespace api.Models
{
    public enum ConversationStep
    {
        None,
        WaitingForDisplayName,
        WaitingForAge,
        WaitingForPhoto,
        WaitingForBio,
        Completed
    }
}