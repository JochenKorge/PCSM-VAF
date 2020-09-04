using System;
using System.Diagnostics;
using MFiles.VAF;
using MFiles.VAF.Common;
using MFiles.VAF.Configuration;
using MFiles.VAF.Core;
using MFiles.VAF.Placeholders;
using MFilesAPI;

namespace ChangePropertyWF
{
    /// <summary>
    /// The entry point for this Vault Application Framework application.
    /// </summary>
    /// <remarks>Examples and further information available on the developer portal: http://developer.m-files.com/. </remarks>
    public class VaultApplication
        : ConfigurableVaultApplicationBase<Configuration>
    {

        /// <summary>
        /// Executed when an object is moved into a workflow state
        /// with alias "WFS.Mitarbeitergespräch.Abgeschlossen".
        /// </summary>
        /// <param name="env">The vault/object environment.</param>
        [StateAction("WFS.Mitarbeitergespräch.Abgeschlossen")]
        public void ChangeProperty(StateEnvironment env)
        {
            if (env.ObjVerEx.GetProperty("PD.NeuesGehalt").Value.IsNULL()) return;

            var NeuesGehalt = env.ObjVerEx.GetProperty("PD.NeuesGehalt").TypedValue;
            
            env.ObjVerEx.GetDirectReference("PD.Mitarbeiter").GetDirectReference("PD.VertraulicheMitarbeiterdaten").SaveProperty("PD.Monatsgehalt", MFDataType.MFDatatypeInteger, NeuesGehalt.Value);

        }

    }
}