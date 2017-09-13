/***************************************************************************
 * COREUTIL.CPP
 *
 * Core Internal Functions used by DSOFILE (PropertySet Code)
 *
 *  Copyright (c)1999-2001 Microsoft Corporation, All Rights Reserved
 *  Written by DSO Office Integration, Microsoft Developer Support
 *
 *  THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
 *  EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
 *  WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 *
 ***************************************************************************/
#include "dsofile.h"
#include <olectl.h>    //needed for OleCreatePictureIndirect

////////////////////////////////////////////////////////////////////
// Core PropertySet Functions
//
////////////////////////////////////////////////////////////////////
// DsoOpenPropertyStorage
//
//  Function takes a PropertySetStorage and returns the desired 
//  PropertyStorage for the FMTID. The function will create a storage
//  if one does not exist (and flags allow).
//
STDAPI DsoOpenPropertyStorage(IPropertySetStorage* pPropSS, REFFMTID fmtid, BOOL fReadOnly, DWORD dwFlags, IPropertyStorage** ppPropStg)
{
    HRESULT hr;
    DWORD dwMode;
    BOOL fNoCreate = ((dwFlags & dsoOptionDontAutoCreate) == dsoOptionDontAutoCreate);
    BOOL fUseANSI = ((dwFlags & dsoOptionUseMBCStringsForNewSets) == dsoOptionUseMBCStringsForNewSets);

    ASSERT(pPropSS); ASSERT(ppPropStg);
    if ((pPropSS == NULL) || (ppPropStg == NULL))
        return E_UNEXPECTED;

    *ppPropStg = NULL;

 // Set the access mode for read/write access...
	dwMode = (fReadOnly ? (STGM_READ | STGM_SHARE_EXCLUSIVE)
					    : (STGM_READWRITE | STGM_SHARE_EXCLUSIVE));

 // We try to open the property set. If this fails, it may be beacuse
 // it doesn't exist so we'll try to create the set...
	hr = pPropSS->Open(fmtid, dwMode, ppPropStg);
	if ((hr == STG_E_FILENOTFOUND) && !fReadOnly && !fNoCreate)
	{
     // FIX -- ADDED BY REQUEST - Feb 1, 2001
     // Outlook 2000/XP doesn't handle Unicode property sets very well. So if we need to
     // create a propset for the caller, allow the caller the ability to set the
     // PROPSETFLAG_ANSI flag on the new set.
     // 
     // The ANSI flag was the default in DsoFile 1.0, but this was changed to Unicode 
     // for version 1.2 to meet request by ASIA/EMEA clients. Unicode should work 
     // but there have been reported problems (in Outlook, Win2K SP2) that indicate
     // clients may want to use the ANSI flag (so it is passed here)...
		hr = pPropSS->Create(fmtid, NULL, (fUseANSI ? PROPSETFLAG_ANSI : 0), dwMode, ppPropStg);

    // If we created with ANSI flag, we must set the code page value to match ACP...
        if (SUCCEEDED(hr) && (fUseANSI) && (*ppPropStg))
		{
			VARIANT vtT; 
            PROPSPEC spc;
            spc.ulKind = PRSPEC_PROPID; spc.propid = PID_CODEPAGE;

         // FIX -- ADDED BY REQUEST - Oct 30, 2001
         // Check for CodePage first. It appears certain configurations choke on modification
         // of the code page. This appears to be a change in OLE32 behavior. Workaround is to
         // check to see if OLE32 has added the code page already during the create, and if so
         // we can skip out on adding it ourselves...
            if (FAILED(DsoReadProperty(*ppPropStg, spc, 0, &vtT)))
            { // If not, we should add it...
                vtT.vt = VT_I4; vtT.lVal = GetACP();
			    DsoWriteProperty(*ppPropStg, spc, 0, &vtT);
            }
        }
        
	}

	return hr;
}

////////////////////////////////////////////////////////////////////
// DsoConvertBlobToVarVector
//
//  Takes a PROPVARIANT BLOB and converts it to VARIANT SAFEARRAY 
//  which can be treated by VB as vector (1dim) Byte Array.
//
STDAPI DsoConvertBlobToVarVector(PROPVARIANT *pVarBlob, VARIANT *pVarByteArray)
{
    HRESULT hr = S_FALSE;
    SAFEARRAY* psa;
    DWORD dwSize;

    if ((pVarBlob == NULL) || (pVarBlob->vt != VT_BLOB) || (pVarByteArray == NULL))
        return E_UNEXPECTED;

 // Identify the size of the blob...
    dwSize = pVarBlob->blob.cbSize;
    if ((dwSize) && (dwSize < 0x800000))
    {
     // Create a vector array the size of the blob...
        psa = SafeArrayCreateVector(VT_UI1, 0, dwSize);
        if (psa != NULL)
        {
         // Copy the data over to the vector...
            BYTE *pbyte = NULL;
            hr = SafeArrayAccessData(psa, (void**)&pbyte);
            if (SUCCEEDED(hr))
            {
                SEH_TRY
                memcpy(pbyte, (BYTE*)(pVarBlob->blob.pBlobData), dwSize);
                SEH_EXCEPT(hr)
                SafeArrayUnaccessData(psa);
            }
        }

        if ((psa) && SUCCEEDED(hr) && (pVarByteArray))
        {
            pVarByteArray->vt = (VT_ARRAY | VT_UI1);
            pVarByteArray->parray = psa;
        }
        else if (psa) SafeArrayDestroy(psa);
    }

    return hr;
}


////////////////////////////////////////////////////////////////////
// DsoConvertCFPicToOleStdPic
//
//  Takes a PROPVARIANT CF (for thumbnail) and converts it to an OLE
//  StdPicture object based on CF type (Metafile or Bitmap). There could
//  be other formats to consider, but not used by Office. We expect WMF or DIB.
//
STDAPI DsoConvertCFPicToOleStdPic(PROPVARIANT *pVarCF, VARIANT *pVarStdPicture)
{
    HRESULT hr = S_FALSE;
    CLIPDATA* pclp;

    if ((pVarCF == NULL) || (pVarCF->vt != VT_CF) || (pVarStdPicture == NULL))
        return E_UNEXPECTED;

    SEH_TRY
 // Convert the data to OLE StdPicture object. We handle the process for
 // thumbnails saved as WMF (Word/Execl) and DIB (PhotoShop)...
	pclp = pVarCF->pclipdata;
	if (((DWORD)*pclp->pClipData) == CF_METAFILEPICT)
	{
		DWORD cbDataSize, cbHeaderSize;
		HMETAFILE hwmf;

		cbHeaderSize = (sizeof(DWORD) + (4 * sizeof(WORD)));
		cbDataSize = pclp->cbSize - cbHeaderSize;

		hwmf = SetMetaFileBitsEx(cbDataSize, (BYTE*)(pclp->pClipData + cbHeaderSize));
		if (NULL != hwmf)
		{
			PICTDESC pds;
			IDispatch* pdisp;

			pds.cbSizeofstruct = sizeof(PICTDESC);
			pds.picType = PICTYPE_METAFILE;
			pds.wmf.hmeta = hwmf;
			pds.wmf.xExt = 3000;
			pds.wmf.yExt = 4000;

			hr = OleCreatePictureIndirect(&pds, IID_IDispatch, TRUE, (void**)&pdisp);
			if (SUCCEEDED(hr))
			{
				pVarStdPicture->vt = VT_DISPATCH;
				pVarStdPicture->pdispVal = pdisp;
			}
		}
	}
	else if ((((DWORD)*pclp->pClipData) == CF_DIB) || (((DWORD)*pclp->pClipData) == CF_BITMAP))
	{
		DWORD dwSize = pclp->cbSize;

		PICTDESC pds;
		HANDLE hBitmap;
		BITMAP bm;
		BITMAPINFOHEADER* pbmh;
		IDispatch* pdisp;

		pbmh = (BITMAPINFOHEADER*)((pclp->pClipData) + 4);

		bm.bmType = 0;
		bm.bmWidth = pbmh->biWidth;
		bm.bmHeight = pbmh->biHeight;
		bm.bmPlanes = pbmh->biPlanes;
		bm.bmBitsPixel = pbmh->biBitCount;
		bm.bmWidthBytes = (pbmh->biSizeImage / bm.bmHeight);

		bm.bmBits = (((pclp->pClipData) + 4) + pbmh->biSize);

		hBitmap = CreateBitmapIndirect(&bm);
		if (hBitmap)
		{
			pds.cbSizeofstruct = sizeof(PICTDESC);
			pds.picType = PICTYPE_BITMAP;
			pds.bmp.hpal = NULL;
			pds.bmp.hbitmap = (HBITMAP)hBitmap;

			hr = OleCreatePictureIndirect(&pds, IID_IDispatch, TRUE, (void**)&pdisp);
			if (SUCCEEDED(hr))
			{
				pVarStdPicture->vt = VT_DISPATCH;
				pVarStdPicture->pdispVal = pdisp;
			}
		}
	}

    SEH_EXCEPT_NULL
    return hr;
}


////////////////////////////////////////////////////////////////////
// DsoReadProperty
//
//  Reads a single property from a given PropertyStorage. Code page is
//  used if we have to translate to/from MBCS to Unicode. We handle one special
//  case for PIDSI_EDITTIME which is a INT64 saved in FILETIME structure. For
//  compatibility we cut it down to seconds and store in LONG (VT_I4). This is
//  OK as long as we don't save it back (which we don't allow in this sample). 
//
STDAPI DsoReadProperty(IPropertyStorage* pPropStg, PROPSPEC spc, WORD wCodePage, VARIANT* pvtResult)
{
	HRESULT     hr;
    PROPVARIANT vtProperty;

	if ((pPropStg == NULL) || (pvtResult == NULL))
		return E_POINTER;

 // Initialize PROPVARIANT...
	PropVariantInit(&vtProperty);

 // Make the call to read the property from the set...
	SEH_TRY

	pvtResult->vt = VT_EMPTY; pvtResult->lVal = 0;
	hr = pPropStg->ReadMultiple(1, &spc, &vtProperty);

	SEH_EXCEPT(hr)

 // If the call succeeded, swap the data into a VARIANT...
    if (SUCCEEDED(hr))
    {
     // Make a selected copy based on the type...
	    switch (vtProperty.vt)
	    {
	    case VT_I4:
	    case VT_UI4: pvtResult->vt = VT_I4; pvtResult->lVal = vtProperty.lVal;
            break;

	    case VT_I2:
	    case VT_UI2: pvtResult->vt = VT_I4;  pvtResult->lVal = vtProperty.iVal;
		    break;

	    case VT_BSTR:
		    pvtResult->vt = VT_BSTR;
            pvtResult->bstrVal = ((vtProperty.bstrVal) ? SysAllocString(vtProperty.bstrVal) : NULL);
		    break;

	    case VT_LPWSTR:
		    pvtResult->vt = VT_BSTR;
            pvtResult->bstrVal = ((vtProperty.pwszVal) ? SysAllocString(vtProperty.pwszVal) : NULL);
		    break;

	    case VT_LPSTR:
		    pvtResult->vt = VT_BSTR;
            pvtResult->bstrVal = DsoConvertToBSTR(vtProperty.pszVal, wCodePage);
		    break;

	    case VT_FILETIME:
            // Check fo special case of edit time...
		    if ((spc.ulKind == PRSPEC_PROPID) && (spc.propid == PIDSI_EDITTIME))
		    {
			    unsigned __int64 ns, secs;
			    ////////////////////////////////////////////////
			    // FIX - 9/27/99 Assign to unsigned __int64 first, then shift...
			    // ns = ft.dwLowDateTime + (ft.dwHighDateTime << 32);
			    //
			    ns = vtProperty.filetime.dwHighDateTime; ns <<= 32;
			    ns += vtProperty.filetime.dwLowDateTime;
			    secs = ns / (10000000);

			    pvtResult->vt = VT_I4;
			    pvtResult->lVal = (LONG)((DWORD)(secs / 60));
		    }
		    else
		    {
			    DATE       dtDate;
			    FILETIME   lft;
			    SYSTEMTIME lst;
			    FILETIME*  pft = &(vtProperty.filetime);

			    if (!((pft->dwLowDateTime == 0) && (pft->dwHighDateTime == 0)))
			    {
				    if (FileTimeToLocalFileTime(pft, &lft))
					    pft = &lft;

				    if (FileTimeToSystemTime(pft, &lst) && 
					    SystemTimeToVariantTime(&lst, &dtDate))
				    {
					    pvtResult->vt = VT_DATE;
					    pvtResult->date = dtDate;
				    }
			    }
		    }
		    break;

	    case VT_BOOL:
		    pvtResult->vt = VT_BOOL; pvtResult->boolVal = vtProperty.boolVal;
		    break;

	    case VT_R4:
		    pvtResult->vt = VT_R4; pvtResult->fltVal = vtProperty.fltVal;
		    break;

	    case VT_R8:
		    pvtResult->vt = VT_R8; pvtResult->dblVal = vtProperty.dblVal;
		    break;

        case VT_CF:
            DsoConvertCFPicToOleStdPic(&vtProperty, pvtResult);
            break;

        case VT_BLOB:
            DsoConvertBlobToVarVector(&vtProperty, pvtResult);
            break;

        default:
            hr = STG_E_INVALIDPARAMETER;
            break;
	    }

    }

 // Clear PropVariant and return...
	PropVariantClear(&vtProperty);
	return hr;
}

////////////////////////////////////////////////////////////////////
// DsoWriteProperty
//
//  Writes a property to the given PropertyStorage. The code page parameter
//  is used to convert string into code page of the property set itself only 
//  if the PROPSETFLAG_ANSI is set. Otherwise we save in Unicode.
//
STDAPI DsoWriteProperty(IPropertyStorage* pPropStg, PROPSPEC spc, WORD wCodePage, VARIANT* pvtValue)
{
	HRESULT        hr;
	PROPVARIANT    vtProperty;
	STATPROPSETSTG statstg;
	BOOL           fUseANSI = FALSE;

 // Check the storage and discover whether it is ANSI only...
	SEH_TRY

	if (SUCCEEDED(pPropStg->Stat(&statstg)))
		fUseANSI = ((statstg.grfFlags & PROPSETFLAG_ANSI) == PROPSETFLAG_ANSI);

	SEH_EXCEPT(hr)

 // We only support certain Variant types...
	switch (pvtValue->vt)
	{
	case VT_I4:
	case VT_UI4:
		vtProperty.vt = VT_I4; vtProperty.lVal = pvtValue->lVal;
		break;

	case VT_I2:
	case VT_UI2:
		vtProperty.vt = VT_I2; vtProperty.iVal = pvtValue->iVal;
		break;

	case VT_BOOL:
		vtProperty.vt = VT_BOOL; vtProperty.boolVal = pvtValue->boolVal;
		break;

	case VT_BSTR:
		if (fUseANSI) // When using ANSI propset, convert to local code page...
		{
			vtProperty.vt = VT_LPSTR;
			vtProperty.pszVal = DsoConvertToMBCS(pvtValue->bstrVal, wCodePage);
		}
		else // Otherwise we save the (Unicode) BSTR...
		{
		  /////////////////////////////////////////////////////////////////////
		  // BUG (6/30/01): Changed from saving directly as BSTR to LPWSTR since
		  // Win2K SP2 introduced bug with VT_BSTR types and does not show them
		  // correctly in the UI. We just copy string before handing to OLE.
			vtProperty.vt = VT_LPWSTR;
			vtProperty.pwszVal = DsoConvertToCoTaskMemStr(pvtValue->bstrVal);
		}
		break;

	case VT_DATE: // Date/time values should always be saved as UTC...
		{
			FILETIME utc = {0,0};
			FILETIME lft;
			SYSTEMTIME lst;
			if ((0 != pvtValue->date) && 
				(VariantTimeToSystemTime(pvtValue->date, &lst)) &&
				(SystemTimeToFileTime(&lst, &lft)))
			{
				if (!LocalFileTimeToFileTime(&lft, &utc))
					utc = lft;
			}
			vtProperty.vt = VT_FILETIME;
			vtProperty.filetime = utc;
		}
		break;

	case VT_R4:
		vtProperty.vt = VT_R4;
		vtProperty.fltVal = pvtValue->fltVal;
		break;

	case VT_R8:
		vtProperty.vt = VT_R8;
		vtProperty.dblVal = pvtValue->dblVal;
		break;

	default:
		return E_INVALIDARG; //unsupportted type...
	}

 // Do the Write operation to the given IPropertySet...
	SEH_TRY
    hr = pPropStg->WriteMultiple(1, &spc, &vtProperty, ((spc.ulKind == PRSPEC_LPWSTR) ? 0x2001 : NULL));
    SEH_EXCEPT(hr)

	PropVariantClear(&vtProperty);
	return hr;
}

////////////////////////////////////////////////////////////////////
// DsoVarTypeReadable
//
//  Returns TRUE if PROPVARIANT VARTYPE is readable by our basic read
//  function DsoReadProperty. If type is not supported, we should skip it.

STDAPI_(BOOL) DsoVarTypeReadable(VARTYPE vt)
{
    BOOL fReadable = FALSE;
    switch (vt)
    {
	    case VT_I4:
	    case VT_UI4:
	    case VT_I2:
	    case VT_UI2:
	    case VT_BSTR:
	    case VT_LPWSTR:
	    case VT_LPSTR:
	    case VT_FILETIME:
	    case VT_BOOL:
	    case VT_R4:
	    case VT_R8:
        case VT_CF:
        case VT_BLOB: fReadable = TRUE;  break;
    }
    return fReadable;
}

////////////////////////////////////////////////////////////////////
// DsoLoadPropertySetList
//
//   This function take an IPropertyStorage and enumerates all the properties
//   to create a linked list of CDsoDocProperty objects. The linked list is used
//   to cache the data and manipulate it before a save. 
//
//   FUTURE: We ought to consider building prop list first, then call ReadMulitple 
//   with PROPSPEC array to fill in the data in one shot. However, given the properties
//   we allow are realtively small in size, we have not seen a performance benefit to
//   make this worth the extra code and regression risk.
//
STDAPI DsoLoadPropertySetList(IPropertyStorage *pPropStg, WORD *pwCodePage, CDsoDocProperty** pplist)
{
	HRESULT hr;
    CDsoDocProperty* pList = NULL;
    CDsoDocProperty* pLastItem = NULL;
	IEnumSTATPROPSTG* pEnumProp = NULL;
	ULONG fetched;
	STATPROPSTG sps;
	VARIANT vtCodePage;
	VARIANT vtItem;
	BSTR bstrName;
    PROPSPEC spc;
    WORD wCodePage = 0; // Assumes no code page (caller needs to check).

    if ((pPropStg == NULL) || (pplist == NULL))
        return E_UNEXPECTED;

    memset(&sps, 0, sizeof(sps));
    memset(&spc, 0, sizeof(spc));

 // Get Code page for this storage...
    spc.ulKind = PRSPEC_PROPID;
    spc.propid = PID_CODEPAGE;
	if (SUCCEEDED(DsoReadProperty(pPropStg, spc, wCodePage, &vtCodePage)) &&
		((vtCodePage.vt == VT_I4) || (vtCodePage.vt == VT_I2)))
	{
		wCodePage = LOWORD(vtCodePage.lVal);
		TRACE1("  Code Page: %d\n", (DWORD)wCodePage);
	}

 // Handle exceptions as fatal events...
	SEH_TRY
    *pplist = NULL;

 // Get the property enumerator to see what properties are stored...
	hr = pPropStg->Enum(&pEnumProp);
    if (SUCCEEDED(hr) && (pEnumProp))
    {
		while (SUCCEEDED(hr) && (pEnumProp->Next(1, &sps, &fetched) == S_OK))
		{
         // We don't handle VECTOR data in this sample. And the PROPVARIANT
         // data types we handle are limited to just a subset we can convert
         // to VB supportted types (variant arrays and ole picdisp)...
            if (((sps.vt & VT_VECTOR) != VT_VECTOR) && 
                DsoVarTypeReadable((sps.vt & VT_TYPEMASK)))
            {
                spc.ulKind = PRSPEC_PROPID;
                spc.propid = sps.propid;

             // Read in the property based on the PROPID...
			    hr = DsoReadProperty(pPropStg, spc, wCodePage, &vtItem);
			    if (SUCCEEDED(hr))
			    {
                 // If we got the data, make the property object to hold it
                 // and append last item to link the list (
                    bstrName = ((sps.lpwstrName) ? SysAllocString(sps.lpwstrName) : NULL);
				    pLastItem = pList;

                    pList = CDsoDocProperty::CreateObject(bstrName, spc.propid, &vtItem, FALSE, pLastItem);
					if (pList == NULL) { hr = E_OUTOFMEMORY; pList = pLastItem;}

				    if (bstrName) SysFreeString(bstrName);
				    VariantClear(&vtItem);
			    }
#ifdef _DEBUG
				else
				{
					TRACE1(" ** DsoReadProperty FAILED hr = 0x%X\n", hr);
				}
#endif
            }
            // else we just skip it...

			if (sps.lpwstrName)
				CoTaskMemFree(sps.lpwstrName);
		}

	 // If we successfully get list, return to caller...
		if (SUCCEEDED(hr)) 
		{
			*pplist = pList;
			if (pwCodePage) *pwCodePage = wCodePage;
		}
		else
		{ // If not, try to clean up...
			while (pList)
			{
				pLastItem = pList->GetNextProperty();
				pList->Disconnect(); pList = pLastItem;
			}
		}
    }

    SEH_EXCEPT(hr)

 // Release obtained interface.
	RELEASE_INTERFACE(pEnumProp);
	return hr;
}

////////////////////////////////////////////////////////////////////
// DsoSavePropertySetList
//
//   Takes a linked list of CDsoDocProperty objects and writes those that
//   have changed back to the IPropertyStorage. It will also call DeleteMultiple
//   on any item marked as deleted on save.
//
STDAPI DsoSavePropertySetList(IPropertyStorage *pPropStg, WORD wCodePage, CDsoDocProperty* plist, ULONG *pcSavedItems)
{
    HRESULT hr = S_FALSE;
    CDsoDocProperty* pitem = plist;
    VARIANT *pvt;
    BSTR bstrName = NULL;
    ULONG cItemsChanged = 0;
    PROPSPEC spc;

    if ((pPropStg == NULL) || (plist == NULL))
        return E_UNEXPECTED;

 // Loop through each item in the list...
    while (pitem)
    {
     // If the item is removed, remove it from the document...
        if (pitem->IsRemoved())
        {
         // We only need to remove it if it already exists. If
         // this is an item wehen added then deleted before save,
         // we don't need to do anything...
            if (pitem->IsNewItem() == FALSE)
            {                
             // Determine if item is known by name or by id...
                pitem->get_Name(&bstrName);
                if (bstrName)
                {
                    spc.ulKind = PRSPEC_LPWSTR;
                    spc.lpwstr = bstrName;
                }
                else
                {
                    spc.ulKind = PRSPEC_PROPID;
                    spc.propid = pitem->GetID();

                    if (spc.propid == 0)
						{ ODS(" ** Bad Propid!\n"); hr = E_UNEXPECTED; break; }
                }
            
             // Now remove the item...
                hr = pPropStg->DeleteMultiple(1, &spc);

             // Break out if error occurred...
                if (FAILED(hr)) { TRACE1(" ** DeleteMultiple FAILED hr=0x%X\n", hr); break;}

             // Since we changed an item in the file, we need 
             // to increment the count...
                pitem->OnRemoveComplete();
                ++cItemsChanged;
            }
        }
        else if (pitem->IsDirty())
        {
     // If the item is dirty, try to save it now...
            pvt = pitem->GetDataPtr();
            if ((pvt) && (pvt->vt != VT_EMPTY))
            {
             // Determine if we should save by name or by id...
                pitem->get_Name(&bstrName);
                if (bstrName)
                {
                    spc.ulKind = PRSPEC_LPWSTR;
                    spc.lpwstr = bstrName;
                }
                else
                {
                    spc.ulKind = PRSPEC_PROPID;
                    spc.propid = pitem->GetID();

                    if (spc.propid == 0)
						{ ODS(" ** Bad Propid!\n"); hr = E_UNEXPECTED; break; }
                }

             // Write the property to the property set...
                hr = DsoWriteProperty(pPropStg, spc, wCodePage, pvt);

                FREE_BSTR(bstrName);

             // Break out if error occurred...
                if (FAILED(hr)){ TRACE1(" ** DsoWriteProperty FAILED hr=0x%X\n", hr); break;}

             // Notify object that it was saved, and bump up
             // the modified item count...
                pitem->OnSaveComplete();
                ++cItemsChanged;
            }
        }

        pitem = pitem->GetNextProperty();
    }

    if (pcSavedItems)
        *pcSavedItems = cItemsChanged;

    return hr;
}

////////////////////////////////////////////////////////////////////////
// Internal Global Functions
//
////////////////////////////////////////////////////////////////////////
// DsoGetTypeInfoEx
//
//  Gets an ITypeInfo from the LIBID specified. Optionally can load and
//  register the typelib from a module resource (if specified). Used to
//  load our typelib on demand.
//
STDAPI DsoGetTypeInfoEx(REFGUID rlibid, LCID lcid, WORD wVerMaj, WORD wVerMin, HMODULE hResource, REFGUID rguid, ITypeInfo** ppti)
{
	HRESULT     hr;
	ITypeLib*   ptlib;

	CHECK_NULL_RETURN(ppti, E_POINTER);
    *ppti = NULL;

 // Try to pull information from the registry...
    hr = LoadRegTypeLib(rlibid, wVerMaj, wVerMin, lcid, &ptlib);

 // If that failed, and we have a resource module to load from,
 // try loading it from the module...
    if (FAILED(hr) && (hResource))
    {
		LPWSTR pwszPath;
        if (FGetModuleFileName(hResource, &pwszPath))
        {
         // Now, load the type library from module resource file...
			hr = LoadTypeLib(pwszPath, &ptlib);

		 // Register library to make things easier next time...
			if (SUCCEEDED(hr))
                RegisterTypeLib(ptlib, pwszPath, NULL);

			DsoMemFree(pwszPath);
		}
    }

 // We have the typelib. Now get the requested typeinfo...
	if (SUCCEEDED(hr))
        hr = ptlib->GetTypeInfoOfGuid(rguid, ppti);

 // Release the type library interface.
    RELEASE_INTERFACE(ptlib);
	return hr;
}

////////////////////////////////////////////////////////////////////////
// DsoGetTypeInfo -- Pre-filled version for our built-in typelib.
//
STDAPI DsoGetTypeInfo(REFGUID rguid, ITypeInfo** ppti)
{
    return DsoGetTypeInfoEx(LIBID_DSOFile, 0,
                DSOFILE_VERSION_MAJOR, DSOFILE_VERSION_MINOR, 
                DllModuleHandle(), rguid, ppti);
}

////////////////////////////////////////////////////////////////////////
// DsoReportError -- Report Error for both ComThreadError and DispError.
//
STDAPI DsoReportError(HRESULT hr, LPWSTR pwszCustomMessage, EXCEPINFO* peiDispEx)
{
    BSTR bstrSource, bstrDescription = NULL;
	ICreateErrorInfo* pcerrinfo;
	IErrorInfo* perrinfo;
	CHAR szError[MAX_PATH];
    UINT nID = 0;

 // Don't need to do anything unless this is an error.
    ASSERT(FAILED(hr));
    if (SUCCEEDED(hr)) return hr;

 // Is this one of our custom errors (if so we will pull description from resource)...
    if ((hr > DSO_E_ERR_BASE) && (hr < DSO_E_ERR_MAX))
        nID = (hr & 0xFF);

 // Set the source name...
    bstrSource = SysAllocString(L"Dsofile.dll");

 // Set the error description...
    if (pwszCustomMessage)
    {
        bstrDescription = SysAllocString(pwszCustomMessage);
    }
    else if ((nID) && LoadString(DllModuleHandle(), nID, szError, sizeof(szError)))
    {
        bstrDescription = DsoConvertToBSTR(szError, CP_ACP);
    }
    
 // Set ErrorInfo so that vtable clients can get rich error information...
	if (SUCCEEDED(CreateErrorInfo(&pcerrinfo)))
    {
		pcerrinfo->SetSource(bstrSource);
        pcerrinfo->SetDescription(bstrDescription);

		if (SUCCEEDED(pcerrinfo->QueryInterface(IID_IErrorInfo, (void**) &perrinfo)))
        {
			SetErrorInfo(0, perrinfo);
			perrinfo->Release();
		}
		pcerrinfo->Release();
	}

 // Fill-in DispException Structure for late-boud clients...
    if (peiDispEx)
    {
        peiDispEx->scode = hr;
        peiDispEx->bstrSource = SysAllocString(bstrSource);
        peiDispEx->bstrDescription = SysAllocString(bstrDescription);
    }

 // Free temp strings...
    if (bstrDescription) SysFreeString(bstrDescription);
    if (bstrSource) SysFreeString(bstrSource);

 // We always return error passed (so caller can chain this in return call).
    return hr;
}

////////////////////////////////////////////////////////////////////////
// Heap Allocation (Uses CoTaskMemAlloc)
//
STDAPI_(LPVOID) DsoMemAlloc(DWORD cbSize)
{
    CHECK_NULL_RETURN(v_hPrivateHeap, NULL);
    return HeapAlloc(v_hPrivateHeap, 0, cbSize);
}

STDAPI_(void) DsoMemFree(LPVOID ptr)
{
    if ((v_hPrivateHeap) && (ptr))
        HeapFree(v_hPrivateHeap, 0, ptr);
}

void * _cdecl operator new(size_t size){ return DsoMemAlloc(size);}
void  _cdecl operator delete(void *ptr){ DsoMemFree(ptr); }

////////////////////////////////////////////////////////////////////////
// String Manipulation Functions
//
////////////////////////////////////////////////////////////////////////
// DsoConvertToUnicodeEx
//
STDAPI DsoConvertToUnicodeEx(LPCSTR pszMbcsString, DWORD cbMbcsLen, LPWSTR pwszUnicode, DWORD cbUniLen, WORD wCodePage)
{
	DWORD cbRet;
	UINT iCode = CP_ACP;

	if (IsValidCodePage((UINT)wCodePage))
		iCode = (UINT)wCodePage;

	CHECK_NULL_RETURN(pwszUnicode,    E_POINTER);
	pwszUnicode[0] = L'\0';

	CHECK_NULL_RETURN(pszMbcsString,  E_POINTER);
	CHECK_NULL_RETURN(cbMbcsLen,      E_INVALIDARG);
	CHECK_NULL_RETURN(cbUniLen,       E_INVALIDARG);

	cbRet = MultiByteToWideChar(iCode, 0, pszMbcsString, cbMbcsLen, pwszUnicode, cbUniLen);
	if (cbRet == 0)	return HRESULT_FROM_WIN32(GetLastError());

	pwszUnicode[cbRet] = L'\0';
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// DsoConvertToMBCSEx
//
STDAPI DsoConvertToMBCSEx(LPCWSTR pwszUnicodeString, DWORD cbUniLen, LPSTR pszMbcsString, DWORD cbMbcsLen, WORD wCodePage)
{
	DWORD cbRet;
	UINT iCode = CP_ACP;

	if (IsValidCodePage((UINT)wCodePage))
		iCode = (UINT)wCodePage;

	CHECK_NULL_RETURN(pszMbcsString,     E_POINTER);
	pszMbcsString[0] = L'\0';

	CHECK_NULL_RETURN(pwszUnicodeString, E_POINTER);
	CHECK_NULL_RETURN(cbMbcsLen,         E_INVALIDARG);
	CHECK_NULL_RETURN(cbUniLen,          E_INVALIDARG);

	cbRet = WideCharToMultiByte(iCode, 0, pwszUnicodeString, -1, pszMbcsString, cbMbcsLen, NULL, NULL);
	if (cbRet == 0)	return HRESULT_FROM_WIN32(GetLastError());

	pszMbcsString[cbRet] = '\0';
	return S_OK;
}

////////////////////////////////////////////////////////////////////////
// DsoConvertToCoTaskMemStr
//
STDAPI_(LPWSTR) DsoConvertToCoTaskMemStr(BSTR bstrString)
{
    LPWSTR pwsz;
    ULONG cbLen;

	CHECK_NULL_RETURN(bstrString, NULL);

    cbLen = SysStringLen(bstrString);
    pwsz = (LPWSTR)CoTaskMemAlloc((cbLen * 2) + sizeof(WCHAR));
    if (pwsz)
    {
        memcpy(pwsz, bstrString, (cbLen * 2));
        pwsz[cbLen] = L'\0'; // Make sure it is NULL terminated.
    }

    return pwsz;
}

////////////////////////////////////////////////////////////////////////
// DsoConvertToMBCS -- NOTE: This returns CoTaskMemAlloc string!
//
STDAPI_(LPSTR) DsoConvertToMBCS(LPCWSTR pwszUnicodeString, WORD wCodePage)
{
	LPSTR psz = NULL;
	UINT cblen, cbnew;

	CHECK_NULL_RETURN(pwszUnicodeString, NULL);

	cblen = lstrlenW(pwszUnicodeString);
	cbnew = ((cblen + 1) * sizeof(WCHAR));
	psz = (LPSTR)CoTaskMemAlloc(cbnew);
	if ((psz) && FAILED(DsoConvertToMBCSEx(pwszUnicodeString, cblen, psz, cbnew, wCodePage)))
	{
		CoTaskMemFree(psz);
		psz = NULL;
	}

	return psz;
}

////////////////////////////////////////////////////////////////////////
// DsoConvertToBSTR 
//
STDAPI_(BSTR) DsoConvertToBSTR(LPCSTR pszAnsiString, WORD wCodePage)
{
	BSTR bstr = NULL;
	UINT cblen, cbnew;
    LPWSTR pwsz;

	CHECK_NULL_RETURN(pszAnsiString, NULL);

	cblen = lstrlen(pszAnsiString);
	if ((cblen > 0) && (*pszAnsiString != '\0'))
	{
		cbnew = ((cblen + 1) * sizeof(WCHAR));
		pwsz = (LPWSTR)DsoMemAlloc(cbnew);
		if (pwsz) 
		{
			if (SUCCEEDED(DsoConvertToUnicodeEx(pszAnsiString, cblen, pwsz, cbnew, wCodePage)))
				bstr = SysAllocString(pwsz);

			DsoMemFree(pwsz);
		}
	}

	return bstr;
}

///////////////////////////////////////////////////////////////////////////////////
// DsoCompareStrings
//
//  Calls CompareString API using Unicode version (if available on OS). Otherwise,
//  we have to thunk strings down to MBCS to compare. This is fairly inefficient for
//  Win9x systems that don't handle Unicode, but hey...this is only a sample.
//
STDAPI_(UINT) DsoCompareStrings(LPCWSTR pwsz1, LPCWSTR pwsz2)
{
	UINT iret;
	LCID lcid = GetThreadLocale();
	UINT cblen1, cblen2;

    typedef INT (WINAPI *PFN_CMPSTRINGW)(LCID, DWORD, LPCWSTR, INT, LPCWSTR, INT);
    static PFN_CMPSTRINGW s_pfnCompareStringW = NULL;

 // Check that valid parameters are passed and then contain somethimg...
	if ((pwsz1 == NULL) || ((cblen1 = lstrlenW(pwsz1)) == 0))
		return CSTR_LESS_THAN;

	if ((pwsz2 == NULL) || ((cblen2 = lstrlenW(pwsz2)) == 0))
		return CSTR_GREATER_THAN;

 // If the string is of the same size, then we do quick compare to test for
 // equality (this is slightly faster than calling the API, but only if we
 // expect the calls to find an equal match)...
	if (cblen1 == cblen2)
	{
		for (iret = 0; iret < cblen1; iret++)
		{
			if (pwsz1[iret] == pwsz2[iret])
				continue;

			if (((pwsz1[iret] >= 'A') && (pwsz1[iret] <= 'Z')) &&
				((pwsz1[iret] + ('a' - 'A')) == pwsz2[iret]))
				continue;

			if (((pwsz2[iret] >= 'A') && (pwsz2[iret] <= 'Z')) &&
				((pwsz2[iret] + ('a' - 'A')) == pwsz1[iret]))
				continue;

			break; // don't continue if we can't quickly match...
		}

		// If we made it all the way, then they are equal...
		if (iret == cblen1)
			return CSTR_EQUAL;
	}

 // Now ask the OS to check the strings and give us its read. (We prefer checking
 // in Unicode since this is faster and we may have strings that can't be thunked
 // down to the local ANSI code page)...
	if (v_fRunningOnNT) 
    {
		iret = CompareStringW(lcid, NORM_IGNORECASE | NORM_IGNOREWIDTH, pwsz1, cblen1, pwsz2, cblen2);
	}
	else
	{
	 // If we are on Win9x, we don't have much of choice (thunk the call)...
		LPSTR psz1 = DsoConvertToMBCS(pwsz1, CP_ACP);
		LPSTR psz2 = DsoConvertToMBCS(pwsz2, CP_ACP);
		iret = CompareString(lcid, NORM_IGNORECASE,	psz1, -1, psz2, -1);
		CoTaskMemFree(psz2);
		CoTaskMemFree(psz1);
	}

	return iret;
}

////////////////////////////////////////////////////////////////////////
// Unicode Win32 API wrappers (handles Unicode/ANSI convert for Win98/ME)
//
////////////////////////////////////////////////////////////////////////
// FFindQualifiedFileName
//
STDAPI_(BOOL) FFindQualifiedFileName(LPCWSTR pwszFile, LPWSTR pwszPath, ULONG *pcPathIdx)
{
    DWORD dwRet;

	if (v_fRunningOnNT)  // Windows NT/2000/XP
	{
		LPWSTR lpwszFilePart = NULL;
		dwRet = SearchPathW(NULL, pwszFile, NULL, MAX_PATH, pwszPath, &lpwszFilePart);
        if ((0 == dwRet || dwRet > MAX_PATH)) return FALSE;
        if (pcPathIdx) *pcPathIdx = (((ULONG)lpwszFilePart - (ULONG)pwszPath) / 2);
	}
	else
	{
        CHAR szBuffer[MAX_PATH];
		LPSTR lpszFilePart = NULL;

		LPSTR szFile = DsoConvertToMBCS(pwszFile, CP_ACP);
		CHECK_NULL_RETURN(szFile, E_OUTOFMEMORY);

        szBuffer[0] = '\0';
		dwRet = SearchPathA(NULL, szFile, NULL, MAX_PATH, szBuffer, &lpszFilePart);
        if ((0 == dwRet || dwRet > MAX_PATH)) return FALSE;

        if (pcPathIdx) *pcPathIdx = (ULONG)lpszFilePart - (ULONG)&szBuffer;
        if (FAILED(DsoConvertToUnicodeEx(szBuffer, lstrlen(szBuffer), pwszPath, MAX_PATH, GetACP())))
            return FALSE;
	}

    return TRUE;
}

////////////////////////////////////////////////////////////////////////
// FGetModuleFileName
//
STDAPI_(BOOL) FGetModuleFileName(HMODULE hModule, WCHAR** wzFileName)
{
    LPWSTR pwsz, pwsz2;
    DWORD dw;

    CHECK_NULL_RETURN(wzFileName, FALSE);
    *wzFileName = NULL;

    pwsz = (LPWSTR)DsoMemAlloc(MAX_PATH*2);
    CHECK_NULL_RETURN(pwsz, FALSE);

 // Call GetModuleFileNameW on Win NT/2000/XP/2003 systems...
	if (v_fRunningOnNT)
    {
		dw = GetModuleFileNameW(hModule, pwsz, MAX_PATH);
        if (dw == 0)
        {
            DsoMemFree(pwsz);
            return FALSE;
        }
	}
	else
	{
	 // If we are on Win9x, we don't have much of choice (thunk the call)...
        dw = GetModuleFileName(hModule, (LPSTR)pwsz, MAX_PATH);
        if (dw == 0)
        {
            DsoMemFree(pwsz);
            return FALSE;
        }

        pwsz2 = (LPWSTR)DsoMemAlloc(MAX_PATH*2);
        if (pwsz2 == 0)
        {
            DsoMemFree(pwsz);
            return FALSE;
        }

        if (FAILED(DsoConvertToUnicodeEx((LPSTR)pwsz, dw, pwsz2, MAX_PATH, CP_ACP)))
        {
            DsoMemFree(pwsz2); pwsz2 = NULL;
        }

        DsoMemFree(pwsz);
        pwsz = pwsz2;
    }

    *wzFileName = pwsz;
    return TRUE;
}

////////////////////////////////////////////////////////////////////////
// FSetRegKeyValue
//
STDAPI_(BOOL) FSetRegKeyValue(HKEY hk, WCHAR* pwsz)
{
	LONG lret;

	if (v_fRunningOnNT) 
    {
        lret = RegSetValueExW(hk, NULL, 0, REG_SZ, (BYTE*)pwsz, 
								((lstrlenW(pwsz) + 1) * sizeof(WCHAR)));
    }
    else
	{
		LPSTR psz = DsoConvertToMBCS(pwsz, CP_ACP);
		lret = RegSetValueEx(hk, NULL, 0, REG_SZ, (BYTE*)psz, (lstrlen(psz) + 1));
		CoTaskMemFree(psz);
	}

	return (lret == ERROR_SUCCESS);
}

////////////////////////////////////////////////////////////////////////
// FGetIconForFile
//
typedef HICON (APIENTRY* PFN_ExtractAssociatedIconA)(HINSTANCE, LPSTR, LPWORD);
typedef HICON (APIENTRY* PFN_ExtractAssociatedIconW)(HINSTANCE, LPWSTR, LPWORD);

STDAPI_(BOOL) FGetIconForFile(LPCWSTR pwszFile, HICON *pico)
{
    WORD idx;
    WORD rgBuffer[MAX_PATH];
    static HMODULE s_hShell32 = NULL;
    static PFN_ExtractAssociatedIconA s_pfnExtractAssociatedIconA = NULL;
    static PFN_ExtractAssociatedIconW s_pfnExtractAssociatedIconW = NULL;

    CHECK_NULL_RETURN(pico, FALSE); *pico = NULL;

    if (s_hShell32 == NULL)
    {
        s_hShell32 = GetModuleHandle("shell32.dll");
        CHECK_NULL_RETURN(s_hShell32, FALSE);
    }

    memset(rgBuffer, 0, sizeof(rgBuffer));

	if (v_fRunningOnNT) 
    {
        if (s_pfnExtractAssociatedIconW == NULL)
        {
            s_pfnExtractAssociatedIconW = (PFN_ExtractAssociatedIconW)GetProcAddress(s_hShell32, "ExtractAssociatedIconW");
            CHECK_NULL_RETURN(s_pfnExtractAssociatedIconW, FALSE);
        }

        idx = (lstrlenW(pwszFile) * 2);
        memcpy((BYTE*)rgBuffer, (BYTE*)pwszFile, idx); idx = 0;
        *pico = s_pfnExtractAssociatedIconW(DllModuleHandle(), (LPWSTR)rgBuffer, &idx);
    }
    else
	{
		LPSTR psz;
        if (s_pfnExtractAssociatedIconA == NULL)
        {
            s_pfnExtractAssociatedIconA = (PFN_ExtractAssociatedIconA)GetProcAddress(s_hShell32, "ExtractAssociatedIconA");
            CHECK_NULL_RETURN(s_pfnExtractAssociatedIconA, FALSE);
        }
        
        psz = DsoConvertToMBCS(pwszFile, CP_ACP);
        if (psz)
        {
            idx = lstrlen(psz);
            memcpy((BYTE*)rgBuffer, (BYTE*)psz, idx); idx = 0;
            *pico = s_pfnExtractAssociatedIconA(DllModuleHandle(), (LPSTR)rgBuffer, &idx);
		    CoTaskMemFree(psz);
        }
	}

	return (*pico != NULL);
}

////////////////////////////////////////////////////////////////////////
// Attribute Functions (Win32 MBCS/Unicode stubs)
//
STDAPI_(DWORD) DsoGetFileAttrib(LPCWSTR pwszFile)
{
	DWORD dwAttrib;
	if (v_fRunningOnNT) 
    {
        dwAttrib = GetFileAttributesW(pwszFile);
    }
    else
	{
		LPSTR psz = DsoConvertToMBCS(pwszFile, CP_ACP);
		dwAttrib = GetFileAttributesA(psz);
		CoTaskMemFree(psz);
	}
	return dwAttrib;
}

STDAPI_(BOOL) DsoSetFileAttrib(LPCWSTR pwszFile, DWORD dwAttrib)
{
	BOOL fSuccess;
	if (v_fRunningOnNT) 
    {
        fSuccess = SetFileAttributesW(pwszFile, dwAttrib);
    }
    else
	{
		LPSTR psz = DsoConvertToMBCS(pwszFile, CP_ACP);
		fSuccess = SetFileAttributesA(psz, dwAttrib);
		CoTaskMemFree(psz);
	}
	return fSuccess;
}

////////////////////////////////////////////////////////////////////////
// Shell Metadata Handler Lookup
//
static LPCWSTR __fastcall GetExtensionPart(LPCWSTR pwszFile)
{
	LPWSTR pwsz = (LPWSTR)pwszFile;
	UINT cChars = lstrlenW(pwsz);
	if ((cChars == 0) || (cChars > MAX_PATH))
	{
		pwsz = NULL; // Invalid file path string length, bail out now!
	}
	else
	{
		pwsz = &pwsz[cChars - 1];
		while ((*pwsz != L'.') && (pwsz != pwszFile))
			--pwsz;
	}
	return pwsz;
}

static HKEY __fastcall GetHKCRKey(LPCSTR pszKeyString, LPCSTR pszPart)
{
	HKEY hk = NULL;
	CHAR szRegKey[(MAX_PATH * 2)]; 
 // NOTE: This function is only called from below, so we KNOW that both 
 // pszKeyString and pszPart can't be bigger than MAX_PATH, so the buffer 
 // szRegKey is big enough to hold the new string on stack without overrun.
 // If you call this function from somewhere else, you will have to check 
 // the string lengths before hand or modify this code to be buffer safe.
	wsprintf(szRegKey, pszKeyString, pszPart);
	RegOpenKey(HKEY_CLASSES_ROOT, szRegKey, &hk);
	return hk;
}

STDAPI DsoGetMetaHandler(LPCWSTR pwszFile, LPCLSID lpClsid)
{
	HRESULT hret = REGDB_E_CLASSNOTREG; // Assume no handler
	HKEY hkeyExt, hkeyHandler = NULL;
	LPSTR pszExt;

	if ((pwszFile == NULL) || (*pwszFile == L'\0') || (lpClsid == NULL))
		return E_INVALIDARG;

 // Get the extension for the file...
	pszExt = DsoConvertToMBCS(GetExtensionPart(pwszFile), CP_ACP);
	if (pszExt == NULL) return E_OUTOFMEMORY; 

 // Now get the key that is associated with that extension...
	hkeyExt = GetHKCRKey("%s", pszExt);
	if (hkeyExt)
	{
	 // Check for the handler under that key...
		hkeyHandler = GetHKCRKey("%s\\ShellEx\\PropertyHandler", pszExt);
		if (hkeyHandler == NULL)
		{
			CHAR szType[MAX_PATH];
			DWORD dwT, cb = MAX_PATH; szType[0] = '\0';
		 // If it does exist there, check under the associated type...
			if ((RegQueryValue(hkeyExt, NULL, szType, (LONG*)&cb) == ERROR_SUCCESS) && (cb))
			{
				hkeyHandler = GetHKCRKey("%s\\ShellEx\\PropertyHandler", szType);
				if (hkeyHandler == NULL)
				{
				 // If still no handler, you can check for handler on the "perceivedtype", which normally
				 // is for things like images that can have multiple extensions to single base type...
					cb = MAX_PATH; szType[0] = '\0';
					if ((RegQueryValueEx(hkeyExt, "PerceivedType", NULL, &dwT, (BYTE*)szType, &cb) == ERROR_SUCCESS) &&
						(cb) && (dwT == REG_SZ))
					{
						hkeyHandler = GetHKCRKey("SystemFileAssociations\\%s\\ShellEx\\PropertyHandler", szType);
					}

				}
			}
		}

		RegCloseKey(hkeyExt);
	}

 // If we got a reg key, then there is an handler key, lookup the GUID and provide to the caller... 
	if (hkeyHandler)
	{
		CHAR szGUID[80];
		DWORD cb = 80; szGUID[0] = '\0';
		if ((RegQueryValue(hkeyHandler, NULL, szGUID, (LONG*)&cb) == ERROR_SUCCESS) && (cb))
		{
			BSTR bstrGuid = DsoConvertToBSTR(szGUID, CP_ACP);
			if (bstrGuid)
			{
				hret = CLSIDFromString(bstrGuid, lpClsid);
				SysFreeString(bstrGuid);
			}
		}

		RegCloseKey(hkeyHandler);
	}

	CoTaskMemFree(pszExt);
	return hret;
}

