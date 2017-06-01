1.- Install PowerShell 4.0

	https://social.technet.microsoft.com/wiki/contents/articles/21016.how-to-install-windows-powershell-4-0.aspx

2. Enable PowerShell Remoting (both computer)

	> Enable-PSRemoting -Force
	> Set-Item wsman:\localhost\client\trustedhosts *
	> Restart-Service WinRM

3. PowerShell comandos básicos para manejo de archivos:

	> Get-Host
	(Versión del PowerShell)

	> Get-ChildItem -Path C:\temp
	(Listar archivos)

	> New-Item -ItemType Directory -Force -Path C:\Path\That\May\Or\Not\Exist
	(Crear Directorio)

	> Remove-Item c:\scripts\test.txt
	(Borrar Archivo o Directorio)

	> Rename-Item c:\scripts\test.txt new_name.txt
	(Renombrar archivo)

	> Move-Item c:\scripts\test.zip c:\test
	(Mover Archivo)


----------------------------------------------------
------          DESCOMPRIMIR ARCHIVO          ------
----------------------------------------------------


$shell = New-Object -ComObject Shell.Application
$zip = $shell.NameSpace("C:\temp\del\PipeList.zip")
foreach ($item in $zip.items())
{
   $shell.Namespace("C:\temp\del").copyhere($item)
}

----------------------------------------------------


Ejemplo de parámetro:


<Parameters>
  <Target Computer='localhost' 
          Username='corp-aa\andres.castiglia' 
          Password='Axa0707+'>

    <Command>
      Move-Item c:\temp\test.zip c:\temp\del
    </Command>
  </Target>

  <Log>
    c:\temp\del\filer[##].txt
  </Log>
</Parameters>