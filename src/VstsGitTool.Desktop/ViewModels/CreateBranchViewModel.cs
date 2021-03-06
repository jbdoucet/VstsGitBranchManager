﻿namespace VstsGitTool.Desktop.ViewModels
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Configuration;
	using VstsClient;
	using VstsClient.Model;

	public class CreateBranchViewModel : ViewModelBase
    {
	    private string _branchName;

	    public VstsGitBranch BasedOnBranch { get; set; }

	    public IEnumerable<string> BranchGroups => new[] {"feature/", "hotfix/", "release/"};

		public string BranchGroup { get; set; }

	    public string BranchName
	    {
		    get => _branchName;
		    set
		    {
			    _branchName = value;
			    OnPropertyChanged();
		    }
	    }

	    public string FullBranchName => $"{BranchGroup}{BranchName}";

		public bool LinkToWorkItem { get; set; }

        public bool CanLinkToWorkItem => WorkItem != null;

        public VstsWorkItem WorkItem { get; }

	    public VstsGitRepository Repository { get; set; }

	    public CreateBranchViewModel(IEnumerable<VstsGitBranch> branches, VstsWorkItem vstsWorkItem, VstsGitRepository repository)
        {
	        string branchName = null;

	        var sourceBranch =
		        branches.FirstOrDefault(b => b.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase)) ??
		        branches.FirstOrDefault();

	        if (vstsWorkItem != null)
	        {
		        branchName = GenerateBranchName(vstsWorkItem);
		        sourceBranch = GetSourceBranch(vstsWorkItem, branches);
	        }

			BranchName = branchName;
            BasedOnBranch = sourceBranch;
            LinkToWorkItem = true;
            WorkItem = vstsWorkItem;
	        Repository = repository;

	        BranchGroup = GetBranchGroup(vstsWorkItem);
        }

	    private string GenerateBranchName(VstsWorkItem workItem)
	    {
		    var workItemId = workItem.Id;
		    var workItemTitle = workItem.Title.MakeGitBranchNameSafe();
		    var workItemAssignedTo = workItem.AssignedTo.MakeGitBranchNameSafe();
		    var workItemType = workItem.Type.MakeGitBranchNameSafe();

		    var branchNameFormatString = UserConfiguration.BranchNameFormatString;

		    branchNameFormatString = branchNameFormatString.Replace("{workItemId}", "{0}").Replace("{workItemTitle}", "{1}")
			    .Replace("{workItemType}", "{2}").Replace("{workItemAssignedTo}", "{3}");

		    var branchName = string.Format(branchNameFormatString, workItemId, workItemTitle, workItemType,
			    workItemAssignedTo);

		    var maxBranchNameLength = UserConfiguration.MaxBranchNameLength;

		    if (maxBranchNameLength > 0)
		    {
			    maxBranchNameLength = Math.Min(maxBranchNameLength, branchName.Length);
			    branchName = branchName.Substring(0, maxBranchNameLength);
		    }

		    return branchName;
	    }

	    private VstsGitBranch GetSourceBranch(VstsWorkItem workItem, IEnumerable<VstsGitBranch> branches)
	    {
		    switch (workItem.Type)
		    {
			    case "Product Backlog Item":
				    return branches.Any(b => b.Name.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
					    ? branches.Single(b => b.Name.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
					    : branches.SingleOrDefault(b => b.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase));
			    case "Bug":
				    var bugWorkItemDefaultSourceBranch = UserConfiguration.BugWorkItemDefaultSourceBranch;
				    var defaultBugSourceBranch = branches.SingleOrDefault(b => b.Name.Equals(bugWorkItemDefaultSourceBranch,
					    StringComparison.InvariantCultureIgnoreCase));
					if (defaultBugSourceBranch != null)
					{
						return defaultBugSourceBranch;
					}
					return branches.SingleOrDefault(b => b.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase));
			    case "Task":
				    return branches.Any(b => b.Name.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
					    ? branches.Single(b => b.Name.Equals("develop", StringComparison.InvariantCultureIgnoreCase))
					    : branches.SingleOrDefault(b => b.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase));
				default:
					return branches.SingleOrDefault(b => b.Name.Equals("master", StringComparison.InvariantCultureIgnoreCase));
			}
	    }

		private string GetBranchGroup(VstsWorkItem workItem)
	    {
		    var workItemType = workItem == null ? "Unknown" : workItem.Type;

		    switch (workItemType)
		    {
			    case "Product Backlog Item":
				    return "feature/";
			    case "Bug":
				    var bugWorkItemDefaultBranchGroup = UserConfiguration.BugWorkItemDefaultBranchGroup;
				    var defaultBugBranchGroup = BranchGroups.SingleOrDefault(bg => bg.Equals(bugWorkItemDefaultBranchGroup));
				    if (!string.IsNullOrEmpty(defaultBugBranchGroup))
				    {
					    return defaultBugBranchGroup;
				    }
					return "hotfix/";
			    case "Task":
				    return "task/";
			    default:
				    return "feature/";
		    }
		}
    }
}
