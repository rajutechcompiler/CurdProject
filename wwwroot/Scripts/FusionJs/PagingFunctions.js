class PagingFunctions {
    constructor(SelectedIndex, PageSize, TotalRecords, PerPageRecords) {
        this.SelectedIndex = SelectedIndex;
        this.PageSize = PageSize
        this.VisibleIndexsNo = 3
        this.TotalRecords = TotalRecords
        this.PerPageRecords = PerPageRecords
        this.GotoTextFieldId = "#inputGotoPageNum"
        this.BtnSpanGotoPage = "#spanGotoPage"
        this.InvalidPageNoMsg = "Invalid Page Number"
        this.SelectingIndexId = "#paging-div ul .liIndex"
    }
    // this function are using for load data
    LoadRecord() {
        // this is override function
    }
    // this function are using load next record
    Next() {
        if (this.SelectedIndex == this.PageSize || this.PageSize == 0) {
            return false;
        }

        this.SetActiveIndex(this.SelectedIndex, (this.SelectedIndex + 1))
        this.SelectedIndex = this.SelectedIndex + 1;
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.ResetPaginationAndLoadData()
    }
    // this function are using for load previous record
    Previous() {
        if (this.SelectedIndex == 1 || this.PageSize == 0) {
            return false;
        }
        this.SetActiveIndex(this.SelectedIndex, (this.SelectedIndex - 1))
        this.SelectedIndex = this.SelectedIndex - 1;
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.ResetPaginationAndLoadData()

    }
    //this function are using for go to page number and get reocrd
    GoToPage() {
        var pageNum = $(this.GotoTextFieldId).val();

        if (pageNum == this.SelectedIndex) {
            return false;
        }

        if (pageNum.length > 0) {
            if (parseInt(pageNum) <= this.PageSize) {
                this.SetActiveIndex(this.SelectedIndex, parseInt(pageNum))
                this.SelectedIndex = parseInt(pageNum);
                this.ResetPaginationAndLoadData()
            }
        }
    }
    // this function are using go to first page
    FirstPage() {

        if (this.SelectedIndex == 1) {
            return false;
        }

        if (this.PageSize > 0) {
            this.SetActiveIndex(this.SelectedIndex, 1)
            this.SelectedIndex = 1;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        }
    }
    // this function are using for go to last page
    LastPage() {
        if (this.SelectedIndex == this.PageSize) {
            return false;
        }
        if (this.PageSize > 0) {
            this.SetActiveIndex(this.SelectedIndex, this.PageSize)
            this.SelectedIndex = this.PageSize;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        }
    }
    // It highlight the selected page.
    SetActiveIndex(remove, add) {
        $("#paging-container ul [index='" + remove + "']").removeClass("active");
        $("#paging-container ul [index='" + add + "']").addClass("active");
    }
    GetRecord(obj) {
        try {
            var PageNumber = parseInt($(obj).attr("index"));
            if (PageNumber == this.SelectedIndex) {
                return false;
            }
            this.SelectedIndex = PageNumber;
            $(this.GotoTextFieldId).val(this.SelectedIndex)
            this.ResetPaginationAndLoadData()
        } catch (ex) {
            console.log(ex)
        }
    }
    //this function are call the two function that are load the handsontable data
    //and second function (VisibleIndexs) set reset the pagination UI indexs
    ResetPaginationAndLoadData() {
        this.LoadRecord();
        this.VisibleIndexs()
    }
    SetTextFieldValues() {
        $("#divTotalRecords span").html(this.TotalRecords)
        $("#divPerPageRecord span").html(this.PerPageRecords)
        $("#spanTotalPage").html(this.PageSize)
        //$(this.BtnSpanGotoPage).hide();
        $(this.GotoTextFieldId).val(this.SelectedIndex)
        this.AddCommaToTotalRecords()
    }
    AddCommaToTotalRecords() {
        $("#divTotalRecords span").html(this.TotalRecords.toLocaleString())
    }
    VisibleIndexs() {
        if (this.PageSize > this.VisibleIndexsNo) {
            for (var i = 1; i <= this.PageSize; i = i + this.VisibleIndexsNo) {
                if (this.SelectedIndex >= i && this.SelectedIndex < (i + this.VisibleIndexsNo)) {
                    var elementIndex = 0, end = i + this.VisibleIndexsNo;
                    if (i + this.VisibleIndexsNo > this.PageSize) {
                        end = this.PageSize
                    }

                    for (var j = i; j <= end; j++) {
                        if (j == this.SelectedIndex) {
                            $(this.SelectingIndexId).eq(elementIndex).show().addClass("active").attr("index", j).children("a").html(j)
                        } else {
                            $(this.SelectingIndexId).eq(elementIndex).show().removeClass("active").attr("index", j).children("a").html(j)
                        }
                        elementIndex = elementIndex + 1;
                    }

                    if (this.VisibleIndexsNo - elementIndex > 0) {
                        for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                            $(this.SelectingIndexId).eq(k).hide();
                        }
                    }
                    break;
                }
            }
        } else {
            if (this.PageSize == 0) { return false; }
            var elementIndex = 0;
            for (var j = 0; j < this.PageSize; j++) {
                if (j + 1 == this.SelectedIndex) { $(this.SelectingIndexId).eq(j).show().addClass("active").attr("index", j + 1).children("a").html(j + 1) }
                else { $(this.SelectingIndexId).eq(j).show().removeClass("active").attr("index", j + 1).children("a").html(j + 1) }
                elementIndex = elementIndex + 1;
            }
            if (this.VisibleIndexsNo - elementIndex > 0) {
                for (var k = elementIndex; k <= this.VisibleIndexsNo - 1; k++) {
                    $(this.SelectingIndexId).eq(k).hide();
                }
            }
        }
    }
    GotoPageNoChangeEvt(evt) {
        var goto = $(this.GotoTextFieldId).val();
        if (goto > this.PageSize || goto < 1) {
            showAjaxReturnMessage(this.InvalidPageNoMsg, 'w');
            $(this.GotoTextFieldId).val(this.SelectedIndex);
            return false;
        }
        $(this.BtnSpanGotoPage).click();
        return true;
    }
    GotoPageNoKeypressEvt(evt) {
        var ASCIICode = (evt.which) ? evt.which : evt.keyCode
        var goto = $(this.GotoTextFieldId).val();

        if ((goto > this.PageSize || goto < 1) && goto != "") {
            showAjaxReturnMessage(this.InvalidPageNoMsg, 'w');
            $(this.GotoTextFieldId).val(this.SelectedIndex);
            return false;
        }

        if (ASCIICode == 13) {
            $(this.BtnSpanGotoPage).click();
            return false;
        }
        if (ASCIICode > 31 && (ASCIICode < 48 || ASCIICode > 57)) {
            return false;
        }
        return true;
    }
}
