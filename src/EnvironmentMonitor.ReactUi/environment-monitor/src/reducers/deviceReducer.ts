import { createSlice, type PayloadAction } from "@reduxjs/toolkit";
import type { RootState } from "../setup/appStore";

export interface DeviceFilters {
  locationIdentifiers: string[];
  communicationChannelIds: number[];
  isVirtual?: boolean;
}

export interface DeviceState {
  filters: DeviceFilters;
}

const initialState: DeviceState = {
  filters: {
    locationIdentifiers: [],
    communicationChannelIds: [],
    isVirtual: undefined,
  },
};

export const deviceSlice = createSlice({
  name: "device",
  initialState,
  reducers: {
    setDeviceLocationIdentifiers: (
      state,
      action: PayloadAction<string[]>,
    ) => {
      state.filters.locationIdentifiers = action.payload;
    },
    setDeviceCommunicationChannelIds: (
      state,
      action: PayloadAction<number[]>,
    ) => {
      state.filters.communicationChannelIds = action.payload;
    },
    setDeviceIsVirtual: (
      state,
      action: PayloadAction<boolean | undefined>,
    ) => {
      state.filters.isVirtual = action.payload;
    },
  },
});

export const {
  setDeviceLocationIdentifiers,
  setDeviceCommunicationChannelIds,
  setDeviceIsVirtual,
} = deviceSlice.actions;

export const getDeviceFilters = (state: RootState): DeviceFilters =>
  state.deviceInfo.filters;

export default deviceSlice.reducer;
