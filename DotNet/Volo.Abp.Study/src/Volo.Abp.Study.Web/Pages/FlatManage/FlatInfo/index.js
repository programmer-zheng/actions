$(function () {

    $("#FlatInfoFilter :input").on('input', function () {
        dataTable.ajax.reload();
    });

    //After abp v7.2 use dynamicForm 'column-size' instead of the following settings
    //$('#FlatInfoCollapse div').addClass('col-sm-3').parent().addClass('row');

    var getFilter = function () {
        var input = {};
        $("#FlatInfoFilter")
            .serializeArray()
            .forEach(function (data) {
                if (data.value != '') {
                    input[abp.utils.toCamelCase(data.name.replace(/FlatInfoFilter./g, ''))] = data.value;
                }
            })
        return input;
    };

    var l = abp.localization.getResource('Study');

    var service = volo.abp.study.flatManage.flatInfo;
    var createModal = new abp.ModalManager(abp.appPath + 'FlatManage/FlatInfo/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'FlatManage/FlatInfo/EditModal');

    var dataTable = $('#FlatInfoTable').DataTable(abp.libs.datatables.normalizeConfiguration({
        processing: true,
        serverSide: true,
        paging: true,
        searching: false,//disable default searchbox
        autoWidth: false,
        scrollCollapse: true,
        order: [[0, "asc"]],
        ajax: abp.libs.datatables.createAjax(service.getList,getFilter),
        columnDefs: [
            {
                rowAction: {
                    items:
                        [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('Study.FlatInfo.Update'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('Study.FlatInfo.Delete'),
                                confirmMessage: function (data) {
                                    return l('FlatInfoDeletionConfirmationMessage', data.record.id);
                                },
                                action: function (data) {
                                    service.delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                }
            },
            {
                title: l('FlatInfoFlatName'),
                data: "flatName"
            },
            {
                title: l('FlatInfoEnName'),
                data: "enName"
            },
            {
                title: l('FlatInfoDeviceType'),
                data: "deviceType"
            },
            {
                title: l('FlatInfoConfigStr'),
                data: "configStr"
            },
        ]
    }));

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewFlatInfoButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });
});
