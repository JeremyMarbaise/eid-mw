
/* ****************************************************************************

 * eID Middleware Project.
 * Copyright (C) 2008-2011 FedICT.
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License version
 * 3.0 as published by the Free Software Foundation.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this software; if not, see
 * http://www.gnu.org/licenses/.

**************************************************************************** */


#include "cardfactory.h"
#include "thread.h"
#include "unknowncard.h"
#include "common/log.h"
#ifdef CAL_BEID
#include "cardpluginbeid/beidcard.h"
#endif

#include <string>


namespace eIDMW
{
/**
 * The CardConnect() function returns a pointer to a CCard object
 * (that should free()-ed when no longer used) that can be used
 * to communicate to a beidcard.
 */
	CCard *CardConnect(const std::string & csReader, CContext * poContext, CPinpad * poPinpad)
	{
		CCard *poCard = NULL;
		long lErrCode = EIDMW_ERR_CHECK;	// should never be returned
		const char *strReader = NULL;

		if (poContext->m_ulConnectionDelay != 0)
		{
			CThread::SleepMillisecs(poContext->m_ulConnectionDelay);
		}
		// Try if we can connect to the card via a normal SCardConnect()
		SCARDHANDLE hCard = 0;
		try
		{
			hCard = poContext->m_oPCSC.Connect(csReader);
			if (hCard == 0)
			{
				goto done;
			}
		}
		catch(CMWException & e)
		{
			if (e.GetError() == (long)EIDMW_ERR_NO_CARD)
			{
				goto done;
			}
			if (e.GetError() != (long)EIDMW_ERR_CANT_CONNECT && e.GetError() != (long)EIDMW_ERR_CARD_COMM)
			{
				throw;
			}				
			lErrCode = e.GetError();
			hCard = 0;
		}

		strReader = csReader.c_str();

		if (hCard != 0)
		{
			// 1. A card is present and we could connect to it via a normal SCardConnect()

#ifdef CAL_BEID
			if (poCard == NULL)
				poCard = BeidCardGetInstance(PLUGIN_VERSION, strReader, hCard, poContext, poPinpad);
#endif // CAL_BEID

#ifndef __APPLE__
			// If no other CCard subclass could be found
			if (poCard == NULL)
				poCard = new CUnknownCard(hCard, poContext,
							  poPinpad,
							  CByteArray());
#else
			// On Mac OS X, SCardConnect() always works when reading a SIS card on an ACR38U,
			// but not the following SCardTransmit() to read out the data. So we set hCard
			// to 0 which will cause SISCardConnectGetInstance() below to first switch to
			// the correct mode to read out the SIS card.
			hCard = 0;
#endif
		}

		if (hCard == 0)
		{
			// 2. A card is present, but connecting to it is reader-specific (e.g. synchron. cards)

#ifdef CAL_BEID

			if (poCard == NULL)
			{
				poCard = new CUnknownCard(hCard, poContext, poPinpad, CByteArray());
			}

#endif // CAL_BEID

			// If the card is still not recognized here, then it may as well
			// be an badly inserted card, so we'll throw the exception that we
			// caught in the beginning of this function
			if (poCard == NULL)
			{
				throw CMWEXCEPTION(lErrCode);
			}
		}
done:
		return poCard;
	}
}
