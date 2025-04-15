import {
  Backdrop,
  Box,
  CircularProgress,
  Container,
  Typography,
  useMediaQuery,
} from "@mui/material";
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import styled from "styled-components";
import { LeftMenu } from "./LefMenu";
import {
  getHasLeftMenu,
  getIsLeftMenuOpen,
  toggleHasLeftMenu,
  toggleLeftMenuOpen,
} from "../reducers/userInterfaceReducer";

export interface TitlePart {
  text: string;
  to?: string;
}

export interface AppContentWrapperProps {
  children: React.ReactNode;
  isLoading?: boolean;
  leftMenu?: JSX.Element;
  titleComponent?: JSX.Element;
  useSmallTitle?: boolean;
  title: string;
}

const PageContent = styled.div<{ max?: number }>`
  height: 100%;
  display: flex;
  flex-direction: column;
  // margin-top: 16px;
  flex-grow: 1;
  max-width: 100%;
`;

export const AppContentWrapper: React.FC<AppContentWrapperProps> = (props) => {
  const isLoggingIn = false; //useSelector(getIsLoggingIn);
  const dispatch = useDispatch();
  const [menuWidth, setMenuWidth] = useState<number | undefined>(undefined);

  const hasLeftMenu = useSelector(getHasLeftMenu);
  const isLeftMenuOpen = useSelector(getIsLeftMenuOpen);
  const isTallViewport = useMediaQuery("(min-height: 800px)");

  const handleMenuClose = () => {
    dispatch(toggleLeftMenuOpen(false));
  };
  useEffect(() => {
    if (!hasLeftMenu && props.leftMenu) {
      dispatch(toggleHasLeftMenu(true));
    } else if (!props.leftMenu && hasLeftMenu) {
      dispatch(toggleHasLeftMenu(false));
    }
  }, [props.leftMenu, dispatch, hasLeftMenu]);
  const drawTitle = () => {
    return (
      <Box
        sx={{
          display: "flex",
          gap: 1, // Space between grid items
          padding: 1, // Padding around the grid container
          // flexGrow: 1,
          height: "100%",
          flexDirection: "row",
        }}
      >
        <Typography variant={props.useSmallTitle ? "h6" : "h5"}>
          {props.title}
        </Typography>
        {props.titleComponent ? props.titleComponent : null}
      </Box>
    );
  };
  return (
    <>
      <Backdrop
        sx={{ color: "#fff", zIndex: (theme) => theme.zIndex.drawer + 1 }}
        open={props.isLoading || isLoggingIn}
      >
        <CircularProgress color="inherit" />
      </Backdrop>
      <Box
        sx={{
          flexGrow: 1,
          flexDirection: "column",
          minHeight: "calc(100vh - 300px)", // TODO Could be made dynamic
          maxHeight: isTallViewport ? "calc(100vh - 100px)" : undefined,
          overflow: "auto",
          marginLeft: hasLeftMenu && isLeftMenuOpen ? `${menuWidth}px` : "0px",
          display: "flex",
          paddingLeft: 1,
          paddingRight: 1,
        }}
      >
        <Container
          maxWidth="xl"
          sx={{ display: "flex", flexDirection: "column", flexGrow: 1 }}
        >
          {drawTitle()}
          <LeftMenu
            title="filters"
            setMenuWidth={(width: number) => {
              if (width) {
                console.info("Setting width");
                setMenuWidth(width);
              }
            }}
            isOpen={props.leftMenu !== undefined && isLeftMenuOpen}
            onClose={handleMenuClose}
          >
            {props.leftMenu ?? <div></div>}
          </LeftMenu>
          <PageContent>{props.children}</PageContent>
        </Container>
      </Box>
    </>
  );
};
