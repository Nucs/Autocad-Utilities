;Tip1539A:  EL2PL.LSP   Ellipse to Pline  (c)1999, Oleg Khenson

(defun C:E2P (/     A	    CEN	  CLA	CMD   DIS   EN	  END_P1
		END_P2	    ENT	  I	K     LA    LT	  M	MINOR%
		NM    OLDERR	  OS	PELMODE	    SS
	       )
  (defun DXF (CODE ELIST) (cdr (assoc CODE ELIST)))
  (defun EL2PL_ERR (S)
    (if	(/= S "Function cancelled")
      (princ (strcat "\nError: " S))
    )
    (setvar "CMDECHO" CMD)
    (if	OS
      (setvar "OSMODE" OS)
    )
    (setq *ERROR* OLDERR
	  OLDERR  NIL
    )
    (princ)
  )
  (setq	OLDERR	*ERROR*
	*ERROR*	EL2PL_ERR
  )
  (setq CMD (getvar "CMDECHO"))
  (setvar "CMDECHO" 0)
  (princ "\nSelect Ellipses to be converted to PLines: ")
  (setq SS (ssget "I" '((0 . "ELLIPSE")))) (command ".UNDO" "G")
  (if SS
    (progn
      (setq OS (getvar "OSMODE"))
      (setvar "OSMODE" 0)
      (setq I 0
	    K 0
	    M 0
      )
      (repeat (sslength SS)		; for each entity from SS
	(setq A	 (entget (setq NM  (ssname SS I)
			       ENT NM
			 )
		 )
	      LA (DXF 8 A)
	)
	(if (< (cdr (assoc 70 (tblsearch "LAYER" LA))) 4)
	  (progn
	    (if	(= (DXF 0 A) "ELLIPSE")
	      (progn
		(setq LT     (DXF 6 A)
		      CEN    (DXF 10 A)
		      MINOR% (DXF 40 A)
		      END_P1 (mapcar '+ CEN (DXF 11 A))
		      DIS    (distance CEN END_P1)
		      END_P2 (polar
			       CEN
			       (+ (angle CEN END_P1) (/ pi 2.0))
			       (* DIS MINOR%)
			     )
		)
		(setq PELMODE (getvar "PELLIPSE"))
		(setvar "PELLIPSE" 1)
		(setq CLA (getvar "CLAYER"))
		(if (/= CLA LA)
		  (setvar "CLAYER" LA)
		)
		(command ".ELLIPSE"
			 "C"
			 (trans CEN 0 1)
			 (trans END_P1 0 1)
			 (trans END_P2 0 1)
		)
		(setq K (1+ K))
		(setvar "PELLIPSE" PELMODE)
		(entdel ENT)
		(if (/= CLA (getvar "CLAYER"))
		  (setvar "CLAYER" CLA)
		)
		(if
		  (and LT
		       (/= (DXF 6 (tblsearch "LAYER" LA)) LT)
		  )
		   (progn (command ".CHPROP" "L" "" "LT" LT "")
			  (command ".PEDIT" "L" "L" "ON" "")
		   )
		)
	      )
	    )
	  )
	  (setq M (1+ M))
	)
	(setq I (1+ I))
      )      
    )
  )
  (setq *ERROR* OLDERR)
  (command ".UNDO" "E")
  (if OS
    (setvar "OSMODE" OS)
  )
  (setvar "CMDECHO" CMD)
  (princ)
)
