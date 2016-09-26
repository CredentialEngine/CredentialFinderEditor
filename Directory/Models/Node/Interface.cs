using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Node;
using Models.Common;

namespace Models.Node.Interface
{
	//DTO for pointer data
	public class ProfileContext
	{
		public ProfileLink Main { get; set; } //Top-level main profile (e.g., Credential, Organization, etc)
		public ProfileLink Parent { get; set; } //Parent of the profile being worked on (e.g., Condition Profile that "owns" the Jurisdiction Profile being worked on)
		public ProfileLink Profile { get; set; } //Profile being targeted/worked on directly
		public bool IsTopLevel { get; set; } //Indicates whether or not Profile is also Main
	}
	//

	//Data for configuring the editor
	public class EditorSettings
	{
		public EditorSettings()
		{
			MainProfile = new ProfileLink();
			Editor = EditorSettings.EditorType.CREDENTIAL;
			Data = new BaseProfile();
		}
		public enum EditorType { CREDENTIAL, ORGANIZATION, ASSESSMENT, LEARNINGOPPORTUNITY }

		public ProfileLink MainProfile { get; set; }
		public EditorType Editor { get; set; }
		public BaseProfile Data { get; set; }
		public List<BaseProfile> Profiles { get; set; } //Preloaded profile data - used for micro searches
	}
	//

	//Base settings for various types of editor elements
	public class BaseSettings
	{
		public BaseSettings()
		{
			ExtraClasses = new List<string>();
		}

		public string Property { get; set; }
		public string Label { get; set; }
		public string Guidance { get; set; }
		public List<string> ExtraClasses { get; set; } //Extra CSS classes
	}
	//

	//Text input settings
	public class TextInputSettings : BaseSettings
	{
		public TextInputSettings()
		{
			Type = InputType.TEXT;
		}

		public enum InputType { TEXT, DATE, URL, NUMBER, TEXTAREA, HIDDEN }

		public InputType Type { get; set; }
		public string Placeholder { get; set; }
		public bool IsRequired { get; set; }
		public int MinimumLength { get; set; }
	}
	//

	//List input settings
	public class ListInputSettings : BaseSettings
	{
		public ListInputSettings()
		{
			Type = InterfaceType.CHECKBOX_LIST;
			CodeItems = new List<CodeItem>();
			EnumItems = new List<EnumeratedItem>();
			StringItems = new Dictionary<string, string>();
			Attributes = new Dictionary<string, string>();
			IncludeDefaultItem = true;
			EnableUncheck = true;
		}

		public enum InterfaceType { DROPDOWN_LIST, CHECKBOX_LIST, BOOLEAN_CHECKBOX_LIST, BOOLEAN_RADIO_LIST }

		public InterfaceType Type { get; set; }
		public bool HasOtherBox { get; set; }
		public List<CodeItem> CodeItems { get; set; }
		public List<EnumeratedItem> EnumItems { get; set; }
		public Dictionary<string, string> StringItems { get; set; }
		public Dictionary<string, string> Attributes { get; set; }
		public bool IncludeDefaultItem { get; set; } //Inserts a default option of "Select..." with a value of 0 to drop-down lists
		public bool EnableUncheck { get; set; } //Disables unselecting a checkbox if it is loaded as checked - only applies to BOOLEAN_CHECKBOX_LIST
	}
	//

	//Profile settings
	public class ProfileSettings : BaseSettings
	{
		public ProfileSettings()
		{
			Type = ModelType.LIST;
			TabItems = new Dictionary<string, string>();
			IncludeName = true;
			ParentRepeaterId = "{repeaterID}";
		}

		public enum ModelType { LIST, WRAPPER_START, WRAPPER_END }

		public ModelType Type { get; set; }
		public bool IncludeName { get; set; }
		public string Profile { get; set; }
		public string AddText { get; set; }
		//public string ParentEditorName { get; set; }
		public string ParentRepeaterId { get; set; }
		public bool HasTabs { get; set; }
		public Dictionary<string, string> TabItems { get; set; }
	}
	//

	//Micro Search settings
	public class MicroSearchSettings : BaseSettings
	{
		public MicroSearchSettings()
		{
			PageSize = 10;
			PageNumber = 1;
			Previous = "";
			Filters = new List<MicroSearchFilter>();
			StaticSelectorValues = new Dictionary<string, object>();
			HasKeywords = true;
			AllowMultipleSavedItems = true;
			ProfileTemplate = "MicroProfile";
			ParentRepeaterId = "{repeaterID}";
			DoAjaxSave = true;
			SavedItemsHeader = "Saved Items";
			CreateProfileTitle = "Item";
			ProfileType = "";
		}
		public bool HasKeywords { get; set; }
		public bool AllowMultipleSavedItems { get; set; }
		public bool AutoSaveNewParentProfile { get; set; } //If the parent profile is new and hasn't been saved yet, save it before trying to save the selected MicroProfile
		public bool DoAjaxSave { get; set; } //Determines whether or not the search does an immediate save on selection of a result
		public string ParentRepeaterId { get; set; }
		public string ProfileTemplate { get; set; }
		public string Previous { get; set; }
		public string SearchType { get; set; }
		public int PageSize { get; set; }
		public int PageNumber { get; set; }
		public List<MicroSearchFilter> Filters { get; set; }
		public Dictionary<string, object> StaticSelectorValues { get; set; }
		public string SavedItemsHeader { get; set; }
		public bool HasEditProfile { get; set; } //Determines whether or not to show an "Edit Profile" link on results
		public bool HasCreateProfile { get; set; } //Determines whether or not to show a "Create New" button
		public string ProfileType { get; set; } //Indicates which profile to load when "Create New" is clicked 
		public string CreateProfileTitle { get; set; } //Text to show after "Create New" on the Create New button
	}
	//

	//Micro Search Filter
	public class MicroSearchFilter
	{
		public MicroSearchFilter()
		{
			Attributes = new Dictionary<string, string>();
			Items = new Dictionary<string, string>();
		}
		public string Type { get; set; }
		public string FilterName { get; set; }
		public string Placeholder { get; set; }
		public Dictionary<string, string> Attributes { get; set; }
		public Dictionary<string, string> Items { get; set; }
	}
	//

	//Text Value Settings
	public class TextValueSettings : BaseSettings
	{
		public TextValueSettings()
		{
			CodeItems = new List<CodeItem>();
			EnumItems = new List<EnumeratedItem>();
			ValueType = TextInputSettings.InputType.TEXT;
			IncludeSelector = true;
			IncludeOtherBox = true;
		}

		public List<CodeItem> CodeItems { get; set; }
		public List<EnumeratedItem> EnumItems { get; set; }
		public bool IncludeSelector { get; set; }
		public bool IncludeOtherBox { get; set; }
		public string ValueLabel { get; set; }
		public TextInputSettings.InputType ValueType { get; set; }
		public string ValueGuidance { get; set; }
	}
	//

	//Text Value Editor Settings
	public class TextValueEditorSettings : BaseSettings
	{
		public TextValueEditorSettings()
		{
			AddText = "Add New";
			ParentRepeaterId = "{repeaterID}";
			CodeItems = new List<CodeItem>();
			EnumItems = new List<EnumeratedItem>();
			ValueType = TextInputSettings.InputType.TEXT;
			ValuePlaceholder = "Value...";
			OtherPlaceholder = "Other...";
		}

		public string AddText { get; set; }
		public string ParentRepeaterId { get; set; }
		public List<CodeItem> CodeItems { get; set; }
		public List<EnumeratedItem> EnumItems { get; set; }
		public bool HasSelector { get; set; }
		public bool HasOther { get; set; }
		public TextInputSettings.InputType ValueType { get; set; }
		public string ValuePlaceholder { get; set; }
		public string OtherPlaceholder { get; set; }
		public bool RequireOther { get; set; }
		public bool RequireValue { get; set; }
		public int CategoryId { get; set; }
	}
	//

}
