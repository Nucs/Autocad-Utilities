/***************************************************************************
 * MAINENTRY.CPP
 *
 * Main DLL Entry and Required COM Entry Points.
 *
 *  Copyright (c)1999-2001 Microsoft Corporation, All Rights Reserved
 *  Written by DSO Office Integration, Microsoft Developer Support
 *
 *  THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 *  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 *  WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 ***************************************************************************/
#define INITGUID // Init the GUIDS for the project...
#include "dsofile.h"

////////////////////////////////////////////////////////////////////////
// Globals for this module.
//
HINSTANCE         v_hModule;             // DLL module handle
ULONG             v_cLocks;              // Count of server locks
CRITICAL_SECTION  v_csSynch;             // Critical Section
HANDLE            v_hPrivateHeap;        // Private Heap for Component
BOOL              v_fRunningOnNT;        // Flag Set When on Unicode OS
PFN_STGOPENSTGEX  v_pfnStgOpenStorageEx; // StgOpenStorageEx (Win2K/XP only)

////////////////////////////////////////////////////////////////////////
// DllMain -- Main Entry for the Library
//
extern "C" BOOL APIENTRY DllMain(HINSTANCE hDllHandle, DWORD dwReason, LPVOID lpReserved)
{
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
		v_hModule = hDllHandle; v_cLocks = 0;
        v_hPrivateHeap = HeapCreate(0, 0x1000, 0);
        v_fRunningOnNT = ((GetVersion() & 0x80000000) != 0x80000000);
        v_pfnStgOpenStorageEx = ((v_fRunningOnNT) ? (PFN_STGOPENSTGEX)GetProcAddress(GetModuleHandle("OLE32"), "StgOpenStorageEx") : NULL);
		InitializeCriticalSection(&v_csSynch);
		DisableThreadLibraryCalls(hDllHandle);
		break;

	case DLL_PROCESS_DETACH:
        if (v_hPrivateHeap) HeapDestroy(v_hPrivateHeap);
        DeleteCriticalSection(&v_csSynch);
		break;
	}
	return TRUE;
}

////////////////////////////////////////////////////////////////////////
// Standard COM DLL Entry Points
//
////////////////////////////////////////////////////////////////////////
// DllGetClassObject - Gets CF for OleDocumentProperties object
//
STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv)
{
	HRESULT hr;
	CDsoDocumentClassFactory* pcf;

	CHECK_NULL_RETURN(ppv, E_POINTER);
	*ppv = NULL;

 // The only component we can create is OleDocumentProperties...
	if (rclsid != CLSID_OleDocumentProperties)
		return CLASS_E_CLASSNOTAVAILABLE;

 // Create the needed class factory...
	pcf = new CDsoDocumentClassFactory();
	CHECK_NULL_RETURN(pcf, E_OUTOFMEMORY);

 // Get requested interface.
	if (SUCCEEDED(hr = pcf->QueryInterface(riid, ppv)))
        { pcf->LockServer(TRUE); }
    else
        { *ppv = NULL; delete pcf; }

	return hr;
}

////////////////////////////////////////////////////////////////////////
// DllCanUnloadNow - Unload DLL when all locks released
//
STDAPI DllCanUnloadNow()
{
    return ((v_cLocks == 0) ? S_OK : S_FALSE);
}

////////////////////////////////////////////////////////////////////////
// DllRegisterServer - Register DLL for COM
//
STDAPI DllRegisterServer()
{
    HRESULT hr = S_OK;
    HKEY    hk, hk2, hk3;
    DWORD   dwret;
    CHAR    szbuffer[256];
    LPWSTR  pwszModule;
    ITypeInfo* pti;
    
 // If we can't find the path to the DLL, we can't register...
	if (!FGetModuleFileName(v_hModule, &pwszModule))
		return E_UNEXPECTED;

 // Setup the CLSID. This is the most important. If there is a critical failure,
 // we will set HR = GetLastError and return...
    if ((dwret = RegCreateKeyEx(HKEY_CLASSES_ROOT, 
		"CLSID\\"DSOFILE_CLSIDSTR, 0, NULL, 0, KEY_WRITE, NULL, &hk, NULL)) != ERROR_SUCCESS)
	{
		DsoMemFree(pwszModule);
        return HRESULT_FROM_WIN32(dwret);
	}

    lstrcpy(szbuffer, DSOFILE_SHORTNAME);
    RegSetValueEx(hk, NULL, 0, REG_SZ, (BYTE *)szbuffer, lstrlen(szbuffer));

 // Setup the InprocServer32 key...
    dwret = RegCreateKeyEx(hk, "InprocServer32", 0, NULL, 0, KEY_WRITE, NULL, &hk2, NULL);
    if (dwret == ERROR_SUCCESS)
    {
        lstrcpy(szbuffer, "Apartment");
        RegSetValueEx(hk2, "ThreadingModel", 0, REG_SZ, (BYTE *)szbuffer, lstrlen(szbuffer));
        
	 // We call a wrapper function for this setting since the path should be
	 // stored in Unicode to handle non-ANSI file path names on some systems.
	 // This wrapper will convert the path to ANSI if we are running on Win9x.
	 // The rest of the Reg calls should be OK in ANSI since they do not
	 // contain non-ANSI/Unicode-specific characters...
		if (!FSetRegKeyValue(hk2, pwszModule))
            hr = E_ACCESSDENIED;

        RegCloseKey(hk2);
    }
    else hr = HRESULT_FROM_WIN32(dwret);

 // Now Register the ProgID and TypeLib (non-critical)...
	if (SUCCEEDED(hr))
	{
        dwret = RegCreateKeyEx(hk, "ProgID", 0, NULL, 0, KEY_WRITE, NULL, &hk2, NULL);
        if (dwret == ERROR_SUCCESS)
        {
            lstrcpy(szbuffer, DSOFILE_PROGID);
            RegSetValueEx(hk2, NULL, 0, REG_SZ, (BYTE *)szbuffer, lstrlen(szbuffer));
            RegCloseKey(hk2);
        }

        if (RegCreateKeyEx(HKEY_CLASSES_ROOT, DSOFILE_PROGID, 0, NULL, 0, KEY_WRITE, NULL, &hk2, NULL) == ERROR_SUCCESS)
        {
            lstrcpy(szbuffer, DSOFILE_FULLNAME);
            RegSetValueEx(hk2, NULL, 0, REG_SZ, (BYTE *)szbuffer, lstrlen(szbuffer));

            if (RegCreateKeyEx(hk2, "CLSID", 0, NULL, 0, KEY_WRITE, NULL, &hk3, NULL) == ERROR_SUCCESS)
            {
                lstrcpy(szbuffer, DSOFILE_CLSIDSTR);
                RegSetValueEx(hk3, NULL, 0, REG_SZ, (BYTE *)szbuffer, lstrlen(szbuffer));
                RegCloseKey(hk3);
            }
            RegCloseKey(hk2);
        }

     // Register the TypeLib by loading it once...
        hr = DsoGetTypeInfo(CLSID_OleDocumentProperties, &pti);
        if (SUCCEEDED(hr)){ RELEASE_INTERFACE(pti);}
	}

 // We're done...
    RegCloseKey(hk);
	DsoMemFree(pwszModule);
    return hr;
}

////////////////////////////////////////////////////////////////////////
// RegRecursiveDeleteKey (Helper Function for DllUnregisterServer)
//
static HRESULT RegRecursiveDeleteKey(HKEY hkParent, LPCSTR pszSubKey)
{
    HRESULT hr = S_OK;
    HKEY hk;
    DWORD dwret, dwsize;
	FILETIME time ;
    CHAR szbuffer[512];

    dwret = RegOpenKeyEx(hkParent, pszSubKey, 0, KEY_ALL_ACCESS, &hk);
	if (dwret != ERROR_SUCCESS)
		return HRESULT_FROM_WIN32(dwret);

 // Enumerate all of the decendents of this child...
	while (dwsize = 510, (RegEnumKeyEx(hk, 0, szbuffer, &dwsize, NULL, NULL, NULL, &time) == ERROR_SUCCESS))
	{
      // If there are any sub-folders, delete them first (to make NT happy)...
		hr = RegRecursiveDeleteKey(hk, szbuffer);
		if (FAILED(hr)) break;
	}

 // Close the child...
	RegCloseKey(hk);
    if (SUCCEEDED(hr))
    {
     // Delete this child.
        dwret = RegDeleteKey(hkParent, pszSubKey);
        if (dwret != ERROR_SUCCESS)
            hr = HRESULT_FROM_WIN32(dwret);
    }

	return hr;
}

////////////////////////////////////////////////////////////////////////
// DllUnregisterServer
//
STDAPI DllUnregisterServer()
{
    HRESULT hr = RegRecursiveDeleteKey(HKEY_CLASSES_ROOT, "CLSID\\"DSOFILE_CLSIDSTR);
    if (SUCCEEDED(hr))
    {
        RegRecursiveDeleteKey(HKEY_CLASSES_ROOT, "TypeLib\\"DSOFILE_TLIBSTR);
        RegRecursiveDeleteKey(HKEY_CLASSES_ROOT, DSOFILE_PROGID);
    }
    else if (hr == 0x80070002)
    {
     // This means the key does not exist (i.e., the DLL was already 
     // unregistered, so we can return S_OK)...
        hr = S_OK;
    }
	return hr;
}

