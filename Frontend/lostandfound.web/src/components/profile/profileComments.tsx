import {
	addProfileComment,
	deleteProfileComment,
	editProfileComment,
	getProfileComments,
	ProfileCommentResponseType,
} from "commons";
import { useContext, useEffect, useState } from "react";
import { Link, Navigate } from "react-router-dom";
import { start } from "repl";
import { userContext } from "userContext";
import { AiFillDelete } from "react-icons/ai";
import { FiEdit, FiStar } from "react-icons/fi";
import { Rating } from "react-simple-star-rating";

export default function ProfileComments({
	profId,
	me,
}: {
	profId: string;
	me: boolean;
}) {
  const usrCtx = useContext(userContext);
  const [comments, setComments] = useState([] as ProfileCommentResponseType[]);
  const [myCom, setMyCom] = useState(
    undefined as ProfileCommentResponseType | undefined
  );
  const [ldg, setLdg] = useState(true);

  useEffect(() => {
    if (ldg === true) {
      if (usrCtx.user.authToken != null)
        getProfileComments(profId, usrCtx.user.authToken).then((x) => {
          if (x?.comments !== undefined) {
            setComments(x?.comments);
          }
          setMyCom(x?.myComment);
          setLdg(false);
        });
    }
  }, [profId, ldg]);

  const addCom = () => {
    if (!me && myCom?.content === undefined)
      return (
        <>
          <h5 className="text-start">Mój komentarz:</h5>
          <CommentAdder
            userId={profId}
            initRating={0}
            initText=""
            accFunc={(text, rating) =>
              addProfileComment(
                profId,
                { profileRating: rating, content: text },
                usrCtx.user.authToken ?? ""
              ).then((x) => setLdg(true))
            }
            clsFunc={() => {}}
            edit={false}
          />
        </>
      );
    else if (myCom?.content !== undefined) {
      return (
        <>
          <h5 className="text-start">Mój komentarz:</h5>
          <ProfileMyComment
            com={myCom}
            profId={profId}
            refresh={() => setLdg(true)}
          ></ProfileMyComment>
        </>
      );
    } else return <></>;
  };

  return (
    <div className="m-3">
      {addCom()}
      {comments.length > 0 && (
        <h5 className="text-start mt-4">Pozostałe komentarze:</h5>
      )}
      {comments.map((x) => (
        <ProfileComment com={x}></ProfileComment>
      ))}
    </div>
  );
}

export function ProfileComment({ com }: { com: ProfileCommentResponseType }) {
  return (
    <div className="border border-1 border-dark rounded-4 p-1 px-4 text-start row d-flex">
      <div className="col align-self-center">
        <img
          className="img-fluid row ms-1 align-self-center"
          style={{ width: "80px" }}
          src={
            com?.author?.pictureUrl ??
            "https://avatars.dicebear.com/api/bottts/stefan.svg"
          }
          alt="noimg"
        ></img>
        <div className="row">
          <strong>{com.author.username}</strong>
        </div>
      </div>
      <div className="col-8">
        <em className="row text-decoration-underline">
          {com.creationDate.toLocaleDateString()}
        </em>
        <div className="d-block row">{com.content}</div>
      </div>
      <div className="col fs-2 align-self-center">
        <FiStar
          className="mt-2"
          fill="#ffc107"
          color="#ffc107"
          style={{ float: "right" }}
        />
        <span style={{ float: "right" }}>{com.profileRating}</span>
      </div>
    </div>
  );
}
export function ProfileMyComment({
  com,
  profId,
  refresh,
}: {
  com: ProfileCommentResponseType;
  profId: string;
  refresh: () => void;
}) {
  const [ed, setEd] = useState(false);
  const usrCtx = useContext(userContext);
  if (ed === true)
    return (
      <CommentAdder
        userId={profId}
        initRating={com.profileRating}
        initText={com.content ?? ""}
        accFunc={(text: string, rating: number) =>
          editProfileComment(
            profId,
            { profileRating: rating, content: text },
            usrCtx.user.authToken ?? ""
          ).then((x) => refresh())
        }
        clsFunc={() => {
          setEd(false);
        }}
        edit={true}
      />
    );
  return (
    <div className="border border-1 border-primary rounded-4 p-1 px-4 text-start row">
      <div className="col align-self-center">
        <img
          className="img-fluid row ms-1 align-self-center"
          style={{ width: "80px" }}
          src={
            com?.author?.pictureUrl ??
            "https://avatars.dicebear.com/api/bottts/stefan.svg"
          }
          alt="noimg"
        ></img>
        <div className="row mr-0 align-self-center">
          <strong>{com.author.username}</strong>
        </div>
      </div>
      <div className="col-7">
        <em className="row text-decoration-underline">
          {com.creationDate.toLocaleDateString()}
        </em>
        <div className="d-block row">{com.content}</div>
      </div>
      <div className="col fs-2 align-self-center p-0 pt-1">
        <FiStar
          className="mt-2 align-self-center"
          fill="#ffc107"
          color="#ffc107"
          style={{ float: "right" }}
        />
        <span className="align-self-center" style={{ float: "right" }}>
          {com.profileRating}
        </span>
      </div>
      <div className="mt-2 col fs-2 align-self-center p-0 ">
        <button
          style={{ float: "right" }}
          className="btn btn-danger mt-1"
          onClick={() => {
            deleteProfileComment(profId, usrCtx.user.authToken ?? "");
            refresh();
          }}
        >
          <AiFillDelete />
        </button>
        <button
          style={{ float: "right" }}
          className="btn btn-warning m-1"
          onClick={() => setEd(!ed)}
        >
          <FiEdit />
        </button>
      </div>
    </div>
  );
}

export function CommentAdder({
  userId,
  initText,
  initRating,
  accFunc,
  clsFunc,
  edit,
}: {
  userId: string;
  initText: string;
  initRating: number;
  accFunc: (text: string, rating: number) => void;
  clsFunc: () => void;
  edit: boolean;
}) {
  const usrCtx = useContext(userContext);

  const [text, setText] = useState(initText);
  const [stars, setStars] = useState(initRating);
  const [exp, setExp] = useState(edit);

  if (exp === false)
    return (
      <button
        className="btn btn-primary rounded-5 mx-auto"
        onClick={() => setExp(true)}
      >
        +
      </button>
    );

  function close() {
    clsFunc();
    setExp(false);
    setText("");
    setStars(0);
  }

  function add() {
    accFunc(text, stars);
  }

  const handleRating = (rate: number) => {
    setStars(rate);
  };

  return (
    <div className="border border-1 border-primary rounded-4 p-1 px-4 text-start">
      <div className="h-auto">
        <p className="m-1">Zostaw komentarz:</p>
        <textarea
          rows={2}
          className="w-100 d-block m-auto mb-2"
          name="description"
          value={text}
          placeholder="Opis"
          onChange={(e) => setText(e.currentTarget.value)}
        ></textarea>
        <Rating onClick={handleRating} initialValue={stars} />
        <div style={{ float: "right" }}>
          <button
            className="d-inline m-auto btn btn-primary rounded-5"
            onClick={() => {
              add();
              close();
            }}
          >
            {edit && "Zapisz"}
            {!edit && "Dodaj"}
          </button>
          <button
            className="ms-1 d-inline m-auto btn btn-danger rounded-5"
            onClick={() => close()}
          >
            Anuluj
          </button>
        </div>
      </div>
    </div>
  );
}
