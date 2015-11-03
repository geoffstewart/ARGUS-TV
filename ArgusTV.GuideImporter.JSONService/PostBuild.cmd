IF %3 == Release goto end
  MD %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins
  MD %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  
  copy %1ArgusTV.DataContracts.dll %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  copy %1ArgusTV.GuideImporter.Interfaces.dll %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  copy %1ArgusTV.GuideImporter.Interfaces.pdb %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService

  copy %1ArgusTV.GuideImporter.JSONService.dll %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  echo "Copying stuff"
  copy /Y %1ArgusTV.GuideImporter.JSONService.dll "\Program Files (x86)\ARGUS TV\Guide Importer\Plugins\JSONService"
  copy %1ArgusTV.GuideImporter.JSONService.pdb %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  copy %1AvailableChannels.config %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
  copy %1..\..\ArgusTV.GuideImporter.JSONService.dll.config %1..\..\..\ArgusTV.GuideImporter\bin\%3\Plugins\JSONService
:end
  copy %1..\..\ArgusTV.GuideImporter.JSONService.dll.config %1ArgusTV.GuideImporter.SchedulesDirect.dll.config
exit 0
