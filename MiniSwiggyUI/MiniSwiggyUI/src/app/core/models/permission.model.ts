export interface UserModulePermission {
  id?: number;
  userId: number;
  moduleKey: string;
  moduleName: string;
  moduleCategory: string;
  routePath: string;
  iconClass: string;
  isAllowed: boolean;
}

export interface UpdateUserPermissionsRequest {
  userId: number;
  permissions: { moduleKey: string; isAllowed: boolean }[];
}
