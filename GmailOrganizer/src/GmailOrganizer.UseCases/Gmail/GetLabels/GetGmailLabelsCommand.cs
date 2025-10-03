using GmailOrganizer.Core.Models;
namespace GmailOrganizer.UseCases.Gmail.GetLabels;

public record GetGmailLabelsCommand(string GoogleUserId)
  : ICommand<Result<GmailLabelsResult>>;
