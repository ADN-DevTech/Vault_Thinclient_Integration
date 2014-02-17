
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Autodesk.Connectivity.Explorer.Extensibility;
using Autodesk.Connectivity.Extensibility.Framework;
using Autodesk.Connectivity.WebServices;
using Autodesk.Connectivity.WebServicesTools;
using VDF = Autodesk.DataManagement.Client.Framework;


//There are  5 assembly attributes you need to have in your code:
//Items 1-3 are provided by Visual Studio in the AssemblyInfo file.  
//You just need to check that they have accurate data.  
//Items 4 and 5 need to be created by you.

//[assembly: AssemblyCompany("Your Company")]
//[assembly: AssemblyProduct("Your product")]
//[assembly: AssemblyDescription("Your assembly description")]
[assembly: ApiVersion("6.0")]
//Go to Tools -> CreateGUID to generate a new GUID
[assembly: ExtensionId("76491449-B3FB-4570-81C4-17FE48BF50CB")]

namespace ThinClientUrlExtension
{
  public class ThinClientUrlGenerator : IExplorerExtension
  {

    VDF.Vault.Currency.Connections.Connection currentConnection;


    public IEnumerable<CommandSite> CommandSites()
    {

      CommandItem ThinClientUrlCmd = new CommandItem(
        "Autodesk.ADN.PermaLink", "View in Thin Client...")
      {
        NavigationTypes = new SelectionTypeId[] { 
          SelectionTypeId.File, 
          SelectionTypeId.FileVersion, 
          SelectionTypeId.Folder },
        MultiSelectEnabled = false
      };

      ThinClientUrlCmd.Execute += ThinClientUrlCmd_Execute;
      CommandSite permaLinkFileContextSite = new CommandSite(
        "Autodesk.ADN.PermaLinkFileContext", "PermaLinkFileContext")
      {
        DeployAsPulldownMenu = false,
        Location = CommandSiteLocation.FileContextMenu
      };
      permaLinkFileContextSite.AddCommand(ThinClientUrlCmd);

      CommandSite permaLinkFolderContextSite = new CommandSite(
        "Autodesk.ADN.PermaLinkFolderContext", "PermaLinkFolderContext")
      {
        DeployAsPulldownMenu = false,
        Location = CommandSiteLocation.FolderContextMenu
      };
      permaLinkFolderContextSite.AddCommand(ThinClientUrlCmd);

      List<CommandSite> sites = new List<CommandSite>();
      sites.Add(permaLinkFileContextSite);
      sites.Add(permaLinkFolderContextSite);

      return sites;
    }

    void ThinClientUrlCmd_Execute(object sender, CommandItemEventArgs e)
    {

      WebServiceManager webMgr = currentConnection.WebServiceManager;

      ISelection selectedItem = e.Context.CurrentSelectionSet.FirstOrDefault<ISelection>();
      if (selectedItem != null)
      {
        Uri serverUri = new Uri(webMgr.InformationService.Url);
        string url;

        if (selectedItem.TypeId == SelectionTypeId.File)
        {
          File file = webMgr.DocumentService.GetLatestFileByMasterId(selectedItem.Id);
          if (file == null)
          {
            return;
          }

          string[] ids = webMgr.KnowledgeVaultService.GetPersistentIds(
            VDF.Vault.Currency.Entities.EntityClassIds.Files,
           new long[] { file.Id },
           Autodesk.Connectivity.WebServices.EntPersistOpt.Latest);

          string id = ids[0];
          id = id.TrimEnd('=');
          url = string.Format("{0}://{1}/AutodeskTC/{1}/{2}#/Entity/Details?id=m{3}=&itemtype=File",
               serverUri.Scheme, serverUri.Host, currentConnection.Vault, id);

          //Open with default broswer
          Process.Start(url);

          //copy url to clipboard
          Clipboard.SetText(url);
        }

        if (selectedItem.TypeId == SelectionTypeId.Folder)
        {
          Folder folder = webMgr.DocumentService.GetFolderById(selectedItem.Id);
          if (folder == null)
          {
            return;
          }

          string[] ids = webMgr.KnowledgeVaultService.GetPersistentIds(
            VDF.Vault.Currency.Entities.EntityClassIds.Folder,
            new long[] { folder.Id },
            Autodesk.Connectivity.WebServices.EntPersistOpt.Latest);

          string id = ids[0];
          id = id.TrimEnd('=');
          url = string.Format("{0}://{1}/AutodeskTC/{1}/{2}#/Entity/Entities?folder=m{3}=&start=0",
            serverUri.Scheme, serverUri.Host, currentConnection.Vault, id);

          //Open with default broswer
          Process.Start(url);

          //copy url to clipboard
          Clipboard.SetText(url);
        }



      }



    }

    public void OnLogOn(IApplication application)
    {
      currentConnection = application.Connection;

    }

    public IEnumerable<CustomEntityHandler> CustomEntityHandlers()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<DetailPaneTab> DetailTabs()
    {
      throw new NotImplementedException();
    }

    public IEnumerable<string> HiddenCommands()
    {
      throw new NotImplementedException();
    }

    public void OnLogOff(IApplication application)
    {

    }

    public void OnShutdown(IApplication application)
    {

    }

    public void OnStartup(IApplication application)
    {

    }
  }
}
