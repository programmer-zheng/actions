using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Study.Localization;
using Volo.Abp.Application.Services;

namespace Volo.Abp.Study;

/* Inherit your application services from this class.
 */
public abstract class StudyAppService : ApplicationService
{
    protected StudyAppService()
    {
        LocalizationResource = typeof(StudyResource);
    }
}
