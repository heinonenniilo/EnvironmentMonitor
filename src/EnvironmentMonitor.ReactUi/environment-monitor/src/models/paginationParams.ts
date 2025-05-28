export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  orderBy?: string;
  isDescending: boolean;
}

/*
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public string? OrderBy { get; set; }

        public bool IsDescending { get; set; }  

*/
