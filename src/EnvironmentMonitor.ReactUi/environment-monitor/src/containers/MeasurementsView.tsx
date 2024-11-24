import { useDispatch, useSelector } from "react-redux";
import { AppContentWrapper } from "../framework/AppContentWrapper";
import React, { useEffect } from "react";
import { getIsLoggedIn } from "../reducers/userReducer";
import { useApiHook } from "../hooks/apiHook";
import { MeasurementsLeftView } from "../components/MeasurementsLeftView";
import {
  getDevices,
  getSensors,
  setDevices,
  setSensors,
} from "../reducers/measurementReducer";

export const MeasurementsView: React.FC = () => {
  // const user = useSelector(getUserInfo);

  const dispatch = useDispatch();
  const isLoggedIn = useSelector(getIsLoggedIn);
  const measurementApiHook = useApiHook().measureHook;

  const devices = useSelector(getDevices);
  const sensors = useSelector(getSensors);

  useEffect(() => {
    if (isLoggedIn && measurementApiHook && devices.length === 0) {
      measurementApiHook.getDevices().then((res) => {
        dispatch(setDevices(res ?? []));
      });
    }
  }, [isLoggedIn, measurementApiHook, devices, dispatch]);
  return (
    <AppContentWrapper
      titleParts={[{ text: "Measurements" }]}
      leftMenu={
        <MeasurementsLeftView
          onSearch={() => {
            //
          }}
          getSensors={(deviceId: string) => {
            measurementApiHook.getSensors(deviceId).then((res) => {
              dispatch(setSensors(res));
            });
          }}
          devices={devices}
          sensors={sensors}
        ></MeasurementsLeftView>
      }
    >
      <p></p>
    </AppContentWrapper>
  );
};
