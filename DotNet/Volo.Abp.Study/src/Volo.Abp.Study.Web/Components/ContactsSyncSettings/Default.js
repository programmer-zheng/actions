(function ($) {

    $(function () {
        $("#SyncSettingForm").on("submit", function (event) {
            event.preventDefault();
            if (!$(this).valid()) {
                return;
            }
            var form = $(this).serializeFormToObject();

            volo.abp.study.customSetting.contactsSyncSettings.update(form).then(function (result) {
                $(document).trigger("AbpSettingSaved");
            })
        });
        
        
    })
})(jQuery);