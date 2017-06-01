using System;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.Permissions;

namespace SBM.Service
{
    internal class Permissions
    {
        private Permissions()
        {
        }

        public static PermissionSet GetDefault(Context context)
        {
            ////elimina todos los permisos
            //PermissionSet permission = new PermissionSet(PermissionState.None);

            ////agrega permiso para acceder al directorio base
            //string aux = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //permission.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, aux));
            ////FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read,

            ////agrega permiso para acceder al subdirectorio del componente
            //aux = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, processInfo.AssemblyDirectory);
            //permission.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, aux));

            ////agrega permiso para acceder a temp
            //aux = Path.GetTempPath();
            //permission.AddPermission(new FileIOPermission(FileIOPermissionAccess.AllAccess, aux));

            //permission.AddPermission(new SqlClientPermission(PermissionState.Unrestricted));
            //permission.AddPermission(new SmtpPermission(PermissionState.Unrestricted));
            //permission.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags));
            //permission.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
            //permission.AddPermission(new ReflectionPermission(PermissionState.Unrestricted));
            //permission.AddPermission(new WebPermission(PermissionState.Unrestricted));

            ////permiso para oledb
            //permission.AddPermission(new OleDbPermission(PermissionState.Unrestricted));

            PermissionSet permission = new PermissionSet(PermissionState.Unrestricted);

            return permission;
        }
    }
}
