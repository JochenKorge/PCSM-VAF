using System;
using System.Diagnostics;
using System.Linq;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Core;
using MFiles.VAF.Extensions;
using MFilesAPI;

namespace CreateNewObject
{
    /// <summary>
    /// The entry point for this Vault Application Framework application.
    /// </summary>
    /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
    public class VaultApplication
        : ConfigurableVaultApplicationBase<Configuration>
    {

        [EventHandler(MFEventHandlerType.MFEventHandlerAfterCheckInChangesFinalize, ObjectType = "Obj.MA")]

        public void NewMaObjCreated(EventHandlerEnvironment env)

        {

            if (env.ObjVerEx.IsFirstVersion)
            {
                string mitarbeiter = $"{env.ObjVerEx.GetPropertyText("PD.Nachname")}, {env.ObjVerEx.GetPropertyText("PD.Vorname")}";
                env.ObjVerEx.SetProperty("PD.VollstName", MFDataType.MFDatatypeText, mitarbeiter);
                int maObjType = env.Vault.ObjectTypeOperations.GetObjectTypeIDByAlias("Obj.MA"); //need ObjTypeID to get ObjectType for OwnerPropertyDef

                MFPropertyValuesBuilder vmdProperties = new MFPropertyValuesBuilder(env.Vault)
                     .SetClass("CL.VMD")
                     .Add("PD.VMD-Name", MFDataType.MFDatatypeText, $"VMD-{mitarbeiter}")
                     .SetLookup(env.Vault.ObjectTypeOperations.GetObjectType(maObjType).OwnerPropertyDef, env.ObjVer)
                     ;

                int newVMD = env.Vault.ObjectOperations.CreateNewObjectExQuick(
                    env.Vault.ObjectTypeOperations.GetObjectTypeIDByAlias("Obj.VMD"),
                    vmdProperties.Values
                    );

                env.ObjVerEx.SetProperty("PD.VertraulicheMitarbeiterdaten", MFDataType.MFDatatypeUninitialized, newVMD);
                //env.ObjVerEx.SetProperty("PD.FirstCheckin", MFDataType.MFDatatypeBoolean, false);
                env.ObjVerEx.SaveProperties();
            }



            //chown when M-Files User is created
            if (env.ObjVerEx.HasValue("PD.MFUser"))
            {
                env.ObjVerEx.SaveProperty(MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy, MFDataType.MFDatatypeUninitialized, env.ObjVerEx.GetProperty("PD.MFUser"));
                env.ObjVerEx.GetDirectReference("PD.VertraulicheMitarbeiterdaten").SaveProperty(MFBuiltInPropertyDef.MFBuiltInPropertyDefCreatedBy,
                    MFDataType.MFDatatypeUninitialized, env.ObjVerEx.GetProperty("PD.MFUser"));
            }

        }


        [EventHandler(MFEventHandlerType.MFEventHandlerAfterCheckInChangesFinalize, ObjectType = "Obj.VMD")]

        public void vmdObjChanged(EventHandlerEnvironment env)
        {

            //Renaming Obj.MA and Obj.VMD after Personalnummer changed
            if (env.ObjVerEx.HasValue("PD.Personalnummer"))
            {
                if (!env.ObjVerEx.Title.Contains(env.ObjVerEx.GetProperty("PD.Personalnummer").GetValueAsUnlocalizedText()))
                {
                    //Get Owner MA-Object

                    var maObj = env.ObjVerEx.GetOwner();
                    //int version = maObj.Version;

                    string vollstName = $"" +
                        $"{maObj.GetProperty("PD.Nachname").GetValueAsUnlocalizedText()}, " +
                        $"{maObj.GetProperty("PD.Vorname").GetValueAsUnlocalizedText()} (" +
                        $"{env.ObjVerEx.GetProperty("PD.Personalnummer").GetValueAsUnlocalizedText()})"
                        ;

                    //maObj.CheckOut();
                    //string nameold = maObj.GetProperty(0).GetValueAsLocalizedText();
                    //var whatId = maObj.GetProperty("PD.VollstName");
                    maObj.SetProperty("PD.VollstName", MFDataType.MFDatatypeText, $"{vollstName}");
                    maObj.SetProperty(0, MFDataType.MFDatatypeText, $"{vollstName}");
                    //string namenew = maObj.GetProperty(0).GetValueAsLocalizedText();

                    maObj.SetProperty(MFBuiltInPropertyDef.MFBuiltInPropertyDefLastModifiedBy, MFDataType.MFDatatypeLookup, env.CurrentUserID);
                    //maObj.SetProperty("PD.debugtxt", MFDataType.MFDatatypeText, env.CurrentUserID.ToString());
                    maObj.SaveProperties();
                    //maObj.CheckIn();

                    env.ObjVerEx.SaveProperty("PD.VMD-Name", MFDataType.MFDatatypeText, $"VMD - {vollstName}");

                }

            }

        }

    }

}
