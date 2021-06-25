using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace FoxNetSoft.Plugin.Misc.SpeedFilters.Logger
{
    public class InstallLocaleResources
    {
        #region Fields

        private readonly bool _showDebugInfo = false;
        private readonly FNSLogger _fnsLogger;
        private readonly string path;

        #endregion

        #region Ctor
        public InstallLocaleResources(string filepath, bool showDebugInfo = false)
        {
            this.path = filepath;
            _showDebugInfo = showDebugInfo;
            this._fnsLogger = new FNSLogger(showDebugInfo);
        }
        #endregion

        #region Classes

        private class LocaleStringResourceParent : LocaleStringResource
        {
            public LocaleStringResourceParent(XmlNode localStringResource, string nameSpace = "")
            {
                Namespace = nameSpace;
                var resNameAttribute = localStringResource.Attributes["Name"];
                var resValueNode = localStringResource.SelectSingleNode("Value");

                if (resNameAttribute == null)
                {
                    throw new NopException("All language resources must have an attribute Name=\"Value\".");
                }
                var resName = resNameAttribute.Value.Trim();
                if (string.IsNullOrEmpty(resName))
                {
                    throw new NopException("All languages resource attributes 'Name' must have a value.'");
                }
                ResourceName = resName;

                if (resValueNode == null || string.IsNullOrEmpty(resValueNode.InnerText.Trim()))
                {
                    IsPersistable = false;
                }
                else
                {
                    IsPersistable = true;
                    ResourceValue = resValueNode.InnerText.Trim();
                }

                foreach (XmlNode childResource in localStringResource.SelectNodes("Children/LocaleResource"))
                {
                    ChildLocaleStringResources.Add(new LocaleStringResourceParent(childResource, NameWithNamespace));
                }
            }
            public string Namespace { get; set; }
            public IList<LocaleStringResourceParent> ChildLocaleStringResources = new List<LocaleStringResourceParent>();

            public bool IsPersistable { get; set; }

            public string NameWithNamespace
            {
                get
                {
                    var newNamespace = Namespace;
                    if (!string.IsNullOrEmpty(newNamespace))
                    {
                        newNamespace += ".";
                    }
                    return newNamespace + ResourceName;
                }
            }
        }

        private class ComparisonComparer<T> : IComparer<T>, IComparer
        {
            private readonly Comparison<T> _comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }

            public int Compare(object o1, object o2)
            {
                return _comparison((T)o1, (T)o2);
            }
        }

        #endregion

        #region Utils

        public void LogMessage(string message)
        {
            if (this._showDebugInfo)
            {
                this._fnsLogger.LogMessage(message);
            }
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Add a locale resource (if new) or update an existing one
        /// </summary>
        /// <param name="localizationService">Localization service</param>
        /// <param name="languageService">Language service</param>
        /// <param name="resourceName">Resource name</param>
        /// <param name="resourceValue">Resource value</param>
        private void AddOrUpdatePluginLocaleResource(
            ILocalizationService localizationService, ILanguageService languageService,
            string resourceName, string resourceValue, bool isUpdateIfFound = true)
        {
            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (languageService == null)
                throw new ArgumentNullException("languageService");
            if (String.IsNullOrWhiteSpace(resourceName))
                return;
            if (String.IsNullOrWhiteSpace(resourceValue))
                return;

            foreach (var lang in languageService.GetAllLanguages(true))
            {
                var lsr = localizationService.GetLocaleStringResourceByName(resourceName, lang.Id, false);
                if (lsr == null)
                {
                    lsr = new LocaleStringResource()
                    {
                        LanguageId = lang.Id,
                        ResourceName = resourceName,
                        ResourceValue = resourceValue
                    };
                    localizationService.InsertLocaleStringResource(lsr);
                }
                else
                {
                    if (isUpdateIfFound)
                    {
                        lsr.ResourceValue = resourceValue;
                        localizationService.UpdateLocaleStringResource(lsr);
                    }
                }
            }
        }

        private void InstallLanguageResource(bool isUpdateIfFound = true)
        {
            var languageService = EngineContext.Current.Resolve<ILanguageService>();
            var languageRepository = EngineContext.Current.Resolve<IRepository<Language>>();
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();


            if (localizationService == null)
                throw new ArgumentNullException("localizationService");
            if (languageService == null)
                throw new ArgumentNullException("languageService");

            //save resoureces
            foreach (var filePath in System.IO.Directory.EnumerateFiles(CommonHelper.MapPath(path), "*.xml", SearchOption.TopDirectoryOnly))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("InstallLocaleResources. Install. filePath=" + filePath);
                #region Parse resource files (with <Children> elements)
                //read and parse original file with resources (with <Children> elements)
                /*
<Language Name="English" IsRightToLeft="false">
<LocaleResource Name="FoxNetSoft.Plugin.Misc.SpeedFilters.EnableSpeedFilters">
<Value>
  Enable Speed Filters
</Value>
</LocaleResource>                     
                 */
                string resourceName = "", resourceValue = "";

                using (XmlTextReader reader = new XmlTextReader(filePath))
                {
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element: // Узел является элементом.
                                if (reader.Name == "LocaleResource")
                                {
                                    string attribute = reader.GetAttribute("Name");
                                    if (!String.IsNullOrWhiteSpace(attribute))
                                        resourceName = attribute.Trim();
                                }
                                break;
                            case XmlNodeType.Text: // Вывести текст в каждом элементе.
                                if (!String.IsNullOrWhiteSpace(reader.Value))
                                {
                                    resourceValue = reader.Value.Trim();
                                    this.AddOrUpdatePluginLocaleResource(localizationService, languageService,
                                        resourceName, resourceValue, isUpdateIfFound);
                                    sb.AppendLine(String.Format("    ResourceName={0}, ResourceValue={1}", resourceName, resourceValue));
                                }
                                resourceName = "";
                                resourceValue = "";
                                break;
                            case XmlNodeType.EndElement: // Вывести конец элемента.
                                resourceName = "";
                                resourceValue = "";
                                break;
                        }
                    }
                }


                #endregion

                LogMessage(sb.ToString());

            }

        }

        #endregion

        #region Methods

        public void Install()
        {
            InstallLanguageResource(true);
        }

        public void Update()
        {
            InstallLanguageResource(false);
        }

        public void UnInstall(string languageResourceMask)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("InstallLocaleResources. UnInstall. languageResourceMask=" + languageResourceMask);

            if (String.IsNullOrWhiteSpace(languageResourceMask))
                return;
            var _lsrRepository = EngineContext.Current.Resolve<IRepository<LocaleStringResource>>();
            var query = _lsrRepository.Table.Where(l => l.ResourceName.Contains(languageResourceMask)).ToList();
            foreach (var lsr in query)
            {
                sb.AppendLine("InstallLocaleResources. UnInstall. ResourceName=" + lsr.ResourceName + ", ResourceValue=" + lsr.ResourceValue);
                _lsrRepository.Delete(lsr);
            }
            sb.AppendLine("InstallLocaleResources. UnInstall. Ok.");
            LogMessage(sb.ToString());
        }
        #endregion
    }
}
