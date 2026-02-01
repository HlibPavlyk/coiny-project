import {SortCriteria} from "@core/models/api/sort-criteria.model";

export interface SearchRequestModel {
  offset: number;
  count: number;
  searchText?: string;
  columnsSearch?: { [key: string]: any };
  sortBy?: SortCriteria[];
}

